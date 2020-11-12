using CalendarLog.CalCopy.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarLog.CalCopy.Services
{
    public interface ICopyCalendarService
    {
        Task CopyCalendarsAsync(List<CalendarEntryVM> calendars);
    }
}