using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public class CopyCalendarService : ICopyCalendarService
    {
        private readonly CalCopyDbContext _context;
        private readonly INotificationService _notificationService;
        private Regex _communityNumberRegex = new Regex(@"(\d{5})");
        private Action<string> _log;
        private Action<string> _logError;
        public CopyCalendarService(
            CalCopyDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
            _log = msg => _notificationService.Publish(Constants.Channels.CopyCalendarsAsyncLog, new CalendarLogNotification { Message = msg });
            _logError = msg => _notificationService.Publish(Constants.Channels.CopyCalendarsAsyncLog, new CalendarLogNotification { IsError = true, Message = msg });
        }

        public async Task CopyCalendarsAsync(List<CalendarEntryVM> calendars)
        {
            if (!calendars.Any())
            {
                return;
            }

            Settings settings = await _context.Settings.FirstOrDefaultAsync();

            try
            {
                if (!String.IsNullOrEmpty(settings.ProofingFolder))
                {
                    List<CalendarEntryVM> selectedCalendars = calendars
                        .GroupBy(g => g.CommunityID)
                        .Select(s => s.FirstOrDefault())
                        .Select(c => new CalendarEntryVM()
                        {
                            CommunityCode = c.CommunityCode,
                            CommunityName = c.CommunityName,
                            CalendarType = c.CalendarType
                        }).ToList();

                    List<string> ProofingFolder = new List<string>(Directory.EnumerateDirectories(settings.ProofingFolder));

                    foreach (var calendar in selectedCalendars)
                    {
                        try
                        {
                            _notificationService.Publish(Constants.Channels.CopyCalendarsAsync, new CalendarNotification
                            { 
                                Action = CalendarNotification.Actions.Start,
                                CalendarGroup = calendars.Where(cal => cal.CommunityCode == calendar.CommunityCode).ToList()
                            });

                            foreach (var folder in ProofingFolder)
                            {
                                DirectoryInfo d = new DirectoryInfo(folder);

                                if (!_communityNumberRegex.Match(d.Name).Success)
                                {
                                    continue;
                                }

                                string folderName = d.Name;
                                string communityCodeFromFolder = _communityNumberRegex.Match(folderName).Value;

                                if (String.Equals(communityCodeFromFolder, calendar.CommunityCode.ToString()))
                                {
                                    string newFolder = String.Format("{0}{1}", settings.WorkingCalendarFolder + "/", folderName);

                                    if (!Directory.Exists(newFolder))
                                    {
                                        _log(String.Format("Creating: {0}", newFolder));
                                        Directory.CreateDirectory(newFolder);

                                        if (File.Exists(settings.MasterTemplateFile))
                                        {
                                            string templateName = settings.MasterTemplateFile.Substring(settings.MasterTemplateFile.LastIndexOf("/") + 1);

                                            string fileNameTemplate = "10{0}_{1}{2}_{3}_{4}.indd";

                                            if (calendar.CommunityCode > 9999)
                                            {
                                                fileNameTemplate = "{0}_{1}{2}_{3}_{4}.indd";
                                            }

                                            string newTemplateName = String.Format(fileNameTemplate, calendar.CommunityCode, DateTime.Now.AddMonths(1).Month < 10 ? "0" + DateTime.Now.AddMonths(1).Month.ToString() : DateTime.Now.AddMonths(1).Month.ToString(), DateTime.Now.AddMonths(1).Year.ToString().Substring(2), calendar.CommunityName.Replace(" ", ""), settings.ProoferInitials);
                                            if (!File.Exists(String.Format("{0}/{1}", newFolder, newTemplateName)))
                                            {
                                                CopyFile(settings.MasterTemplateFile, Path.Combine(newFolder, newTemplateName));
                                            }
                                        }

                                        List<string> files = new List<string>(Directory.EnumerateFiles(folder));

                                        foreach (var file in files)
                                        {

                                            _log(String.Format("Copying: {0}", file));
                                            string fileName = file.Replace(folder, "");
                                            CopyFile(file, String.Format("{0}{1}", newFolder, fileName));
                                        }

                                        await RenameFolderAsync(calendar, settings.WorkingCalendarFolder + "/", settings.ProoferInitials);

                                        _notificationService.Publish(Constants.Channels.CopyCalendarsAsync, new CalendarNotification
                                        {
                                            Action = CalendarNotification.Actions.Complete,
                                            CalendarGroup = calendars.Where(cal => cal.CommunityCode == calendar.CommunityCode).ToList()
                                        });
                                    }

                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _notificationService.Publish(Constants.Channels.CopyCalendarsAsync, new CalendarNotification
                            {
                                Action = CalendarNotification.Actions.Error,
                                CalendarGroup = calendars.Where(cal => cal.CommunityCode == calendar.CommunityCode).ToList(),
                                Exception = ex.ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logError(ex.ToString());
            }
        }

        private async Task RenameFolderAsync(CalendarEntryVM calendar, string path, string proofer)
        {
            await Task.Run(() => 
            {
                DirectoryInfo root = new DirectoryInfo(path);

                DirectoryInfo matched = Directory.EnumerateDirectories(root.FullName)
                    .Select(folder => new DirectoryInfo(folder))
                    .Where(folder => _communityNumberRegex.Match(folder.Name).Success)
                    .FirstOrDefault(folder => Int32.TryParse(_communityNumberRegex.Match(folder.Name).Value, out int communityCode) && calendar.CommunityCode == communityCode);

                if (matched == null)
                    throw new Exception("Directory not found!");

                string folder = matched.FullName;
                int communityNumber = calendar.CommunityCode;

                _log(String.Format("Old Folder: {0}", folder));
                string communityName = calendar.CommunityName;
                string newFolderName = String.Format("{0}_{1}{2}_{3}_{4}",
                                                     communityNumber < 9999 ? $"10{communityNumber}" : communityNumber.ToString(),
                                                     DateTime.Now.AddMonths(1).Month < 10 ? "0" + DateTime.Now.AddMonths(1).Month.ToString() : DateTime.Now.AddMonths(1).Month.ToString(),
                                                     DateTime.Now.AddMonths(1).Year.ToString().Substring(2),
                                                     communityName,
                                                     proofer);

                string newFolderPath = String.Format("{0}", Path.Combine(root.FullName, newFolderName));

                if (matched.Name == newFolderName)
                    return;

                _log(String.Format("Creating: {0}", newFolderPath));
                Directory.CreateDirectory(newFolderPath);

                List<FileInfo> files = matched.EnumerateFiles().ToList();
                _log(String.Format("Getting list of Files in: {0}", folder));
                foreach (var file in files)
                {
                    _log(String.Format("Copying: {0} From: {1} To: {2}", file.Name, folder, newFolderPath));
                    CopyFile(file.FullName, Path.Combine(newFolderPath, file.Name));
                }

                List<string> newFiles = new List<string>(Directory.EnumerateFiles(newFolderPath));

                _log(String.Format("Comparing Old to New Folder"));
                if (files.Count == newFiles.Count)
                {
                    _log(String.Format("Folders Match! Removing Old Folder"));
                    Directory.Delete(folder, true);
                }
            });
        }

        private void CopyFile(string input, string output)
        {
            FileInfo inputFile = new FileInfo(input);
            FileInfo outputFile = new FileInfo(output);

            using (FileStream inputStream = inputFile.OpenRead())
            using (FileStream outputStream = outputFile.Exists ? outputFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite) : outputFile.Create())
            {
                inputStream.CopyTo(outputStream);

                inputStream.Flush();
                inputStream.Close();
                outputStream.Flush();
                outputStream.Close();
            }
        }
    }
}
