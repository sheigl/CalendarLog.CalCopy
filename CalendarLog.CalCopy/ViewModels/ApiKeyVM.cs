using System;

namespace CalendarLog.CalCopy.ViewModels
{
    public class ApiKeyVM
    {
        public int APIKeysID { get; set; }
        public int UserID { get; set; }
        public string APIKey { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime ExpirationDate { get; set; }
        public string SecretKey { get; set; }
    }
}
