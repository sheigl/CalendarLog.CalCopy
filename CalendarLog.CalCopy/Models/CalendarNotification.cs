using CalendarLog.CalCopy.ViewModels;
using System;
using System.Collections.Generic;

namespace CalendarLog.CalCopy.Models
{
    public class CalendarNotification
    {
        public Actions Action { get; set; }
        public List<CalendarEntryVM> CalendarGroup { get; set; }
        public string Exception { get; set; }

        public enum Actions
        { 
            Start,
            Complete,
            Error
        }
    }
}
