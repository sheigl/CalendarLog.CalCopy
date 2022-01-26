using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.ViewModels;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public class GoogleSheetsClient
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google Sheets API .NET Quickstart";
        private readonly ILogger<GoogleSheetsClient> _logger;
        private readonly CalCopyDbContext _context;

        public GoogleSheetsClient(
            ILogger<GoogleSheetsClient> logger,
            CalCopyDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IEnumerable<StatDocRow>> GetCalendarsAsync()
        {
            Settings settings = await _context.Settings.OrderBy(setting => setting.SettingsId).FirstOrDefaultAsync();
            string spreadsheetId = new Regex(@"/[-\w]{25,}/").Match(settings.StatDocUrl).Value?.Trim('/');

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(                    
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
                _logger.LogInformation("Credential file saved to: " + credPath);
            }

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            string range = "Status!A3:AB";
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = request.Execute();

            if (response.Values == null || !response.Values.Any())
            {
                _logger.LogInformation("No data found.");
                return new StatDocRow[] { };
            }

            return response
                .Values
                .Select(row =>
                {
                    try
                    {
                        return new StatDocRow(row);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.ToString());
                        return null;
                    }
                })
                .Where(row => row != null)
                .ToArray();
        }
    }
}
