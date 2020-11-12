using System;
using System.ComponentModel.DataAnnotations;

namespace CalendarLog.CalCopy.Models
{
	public class Settings
	{
        public int SettingsId { get; set; }
		[Required]
        public string MasterTemplateFile { get; set; }
		[Required]
		public string ProofingFolder { get; set; }
		[Required]
		public string WorkingCalendarFolder { get; set; }
		[Required]
		public string APIKey { get; set; }
		[Required]
		public string APIUrl { get; set; }
		[Required]
		public string SecretKey { get; set; }
		[Required]
		public string ProoferInitials { get; set; }
		public DateTime? CreatedDate { get; set; }
		public DateTime? LastModifiedDate { get; set; }
	}
}
