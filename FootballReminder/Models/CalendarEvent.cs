using System;
using System.Collections.Generic;

namespace FootballReminder.Models
{
    public class CalendarEvent
    {
        public string Summary { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime Datetime { get; set; }
        public TimeSpan Duration { get; set; }
        public Dictionary<string, string> ExtraData { get; set; }
    }
}
