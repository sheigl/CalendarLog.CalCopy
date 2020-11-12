using System;

namespace CalendarLog.CalCopy.ViewModels
{
    public class CalendarEntryVM
    {
        public int EntryID { get; set; }
        public int CommunityID { get; set; }
        public int CommunityCode { get; set; }
        public string CommunityName { get; set; }
        public string CalendarType { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<double> Extra { get; set; }
        public string ActiveMonth { get; set; }
        public string Notes { get; set; }
        public string CalendarOwner { get; set; }
        public bool Completed { get; set; }
        public Nullable<System.DateTime> CompletedDate { get; set; }
        public int UserID { get; set; }
        public int InvoiceID { get; set; }

        public bool StatusProcessing { get; private set; }
        public bool StatusCompleted { get; private set; }
        public bool StatusError { get; private set; }

        public void IsProcessing() => ResetStatus().StatusProcessing = true;
        public void IsComplete() => ResetStatus().StatusCompleted = true;
        public void IsError() => ResetStatus().StatusError = true;

        public CalendarEntryVM ResetStatus()
        {
            StatusProcessing = false;
            StatusCompleted = false;
            StatusError = false;

            return this;
        }
    }
}
