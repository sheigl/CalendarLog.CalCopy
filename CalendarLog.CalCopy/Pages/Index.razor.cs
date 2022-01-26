using CalendarLog.CalCopy.Models;
using CalendarLog.CalCopy.Pages.Components;
using CalendarLog.CalCopy.Services;
using CalendarLog.CalCopy.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Pages
{
    public partial class Index
    {
        //[Inject]
        //public ICalendarLogClient Client { get; set; }

        [Inject]
        public ICopyCalendarService CopyCalendarService { get; set; }

        [Inject]
        public INotificationService NotificationService { get; set; }

        [Inject]
        public GoogleSheetsClient GoogleSheetsClient { get; set; }

        public bool Loading { get; set; }


        public List<StatDocRow> ActiveCalendars { get; set; } = new List<StatDocRow>();
        public string Log { get; set; }
        
        public AlertComponent.AlertOptions AlertOptions { get; set; } =
            new AlertComponent.AlertOptions 
            {
                Show = true,
                AlertType = AlertComponent.AlertTypes.info,
                Title = "Uh oh! There's nothing to copy",
                Message = "Please sync your active calendars to begin copying."
            };

        public bool BatchComplete { get; set; }
        public bool IsCopying { get; set; }

        public async Task OnSyncClickAsync()
        {
            try
            {
                await GoogleSheetsClient.GetCalendarsAsync();

                Loading = true;
                ActiveCalendars = null;//await Client.GetCalendarsByApiKeyAsync();

                if (!ActiveCalendars.Any())
                {
                    AlertOptions.Message = "No active calendars were found.";
                }
                else
                {
                    AlertOptions.AlertType = AlertComponent.AlertTypes.info;
                    AlertOptions.Title = "Ready!";
                    AlertOptions.Message = $"There are '{ActiveCalendars.Count}' calendars ready to copy.";
                }
            }
            catch (System.Exception ex)
            {
                AlertOptions.AlertType = AlertComponent.AlertTypes.danger;
                AlertOptions.Title = "Error!";
                AlertOptions.Message = ex.ToString();
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task OnCopyCalendarsAsync()
        {
            try
            {
                IsCopying = true;
                BatchComplete = false;

                NotificationService.Subscribe(Constants.Channels.CopyCalendarsAsyncLog, notification =>
                {
                    CalendarLogNotification logNotification = notification as CalendarLogNotification;

                    //if (logNotification.IsError)
                    //{
                    //    AlertType = AlertComponent.AlertTypes.danger;
                    //    AlertTitle = "Error!";
                    //    AlertMessage = logNotification.Message;
                    //}
                });

                NotificationService.Subscribe(Constants.Channels.CopyCalendarsAsync, async notification =>
                {
                    CalendarNotification calendarNotification = notification as CalendarNotification;

                    switch (calendarNotification.Action)
                    {
                        case CalendarNotification.Actions.Start:
                            calendarNotification.CalendarGroup.ForEach(cal => cal.IsProcessing());
                            await InvokeAsync(() => StateHasChanged());
                            break;
                        case CalendarNotification.Actions.Complete:
                            calendarNotification.CalendarGroup.ForEach(cal => cal.IsComplete());
                            await InvokeAsync(() => StateHasChanged());
                            break;
                        case CalendarNotification.Actions.Error:
                            calendarNotification.CalendarGroup.ForEach(cal => cal.IsError());
                            await InvokeAsync(() => StateHasChanged());
                            break;
                        default:
                            break;
                    }
                });

                BatchComplete = ActiveCalendars.Count<StatDocRow>(cal => cal.StatusCompleted) == ActiveCalendars.Count;

                await CopyCalendarService.CopyCalendarsAsync(ActiveCalendars);

                if (!BatchComplete)
                {
                    AlertOptions.AlertType = AlertComponent.AlertTypes.warning;
                    AlertOptions.Title = "Oops! Something went wrong.";
                    AlertOptions.Message = $"'{ActiveCalendars.Count<CalendarEntryVM>(cal => cal.StatusCompleted)}' of '{ActiveCalendars.Count}' completed.";

                    ActiveCalendars
                        .Where(cal => cal.StatusProcessing)
                        ?.ToList()
                        .ForEach(cal => cal.ResetStatus());

                    await InvokeAsync(() => StateHasChanged());
                }
                else
                {
                    AlertOptions.AlertType = AlertComponent.AlertTypes.success;
                    AlertOptions.Title = "Done!";
                    AlertOptions.Message = $"'{ActiveCalendars.Count<CalendarEntryVM>(cal => cal.StatusCompleted)}' of '{ActiveCalendars.Count}' completed.";
                }
            }
            finally 
            {
                IsCopying = false;
            }
        }
    }
}
