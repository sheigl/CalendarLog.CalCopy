using System;
using System.Collections.Generic;

namespace CalendarLog.CalCopy.ViewModels
{
    public class StatDocRow
    {
        public string CommunityNumber { get; set; }
        public string CommunityName { get; set; }
        public string CalendarType { get; set; }
        public string LayoutAssingedTo { get; set; }
        public DateTime? LayoutAssignedDate { get; set; }
        public DateTime? LayoutCompletedDate { get; set; }
        public bool StatusCompleted { get; set; }

        public StatDocRow()
        {
        }

        public StatDocRow(IList<object> row)
        {
            CommunityNumber = row[(int)StatDocRowColumn.CommunityNumber].ToString();
            CommunityName = row[(int)StatDocRowColumn.CommunityName].ToString();
            CalendarType = row[(int)StatDocRowColumn.CalendarType].ToString();
            LayoutAssingedTo = row[(int)StatDocRowColumn.LayoutAssingedTo].ToString();

            if (DateTime.TryParse(row[(int)StatDocRowColumn.LayoutAssignedDate].ToString(), out var layoutAssignedDate))
                LayoutAssignedDate = layoutAssignedDate;

            if (DateTime.TryParse(row[(int)StatDocRowColumn.LayoutCompletedDate].ToString(), out var layoutCompletedDate))
                LayoutCompletedDate = layoutCompletedDate;
        }
    }

    public enum StatDocRowColumn
    { 
        CommunityNumber = 0,
        CommunityName = 1,
        CalendarType = 2,
        LayoutAssingedTo = 14,
        LayoutAssignedDate = 15,
        LayoutCompletedDate = 16
    }
}
