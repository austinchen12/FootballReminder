using System.Collections.Generic;
using FootballReminder.Models;

namespace FootballReminder.Services
{
    public interface ICalendarService
    {
        public IEnumerable<CalendarEvent> GetEvents();
        public void AddEvent(CalendarEvent calendarEvent);
        public void AddEvents(IEnumerable<CalendarEvent> calendarEvents);
        public void UpdateEvent(CalendarEvent calendarEvent);
        public void UpdateEvents(IEnumerable<CalendarEvent> calendarEvents);
    }
}
