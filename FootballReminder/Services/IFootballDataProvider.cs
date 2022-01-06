using System.Collections.Generic;
using System.Threading.Tasks;
using FootballReminder.Models;

namespace FootballReminder.Services
{
    public interface IFootballDataProvider
    {
        public Task<IEnumerable<CalendarEvent>> GetHeadToHead(int firstId, int secondId);
        public Task<IEnumerable<CalendarEvent>> GetHeadToHeads(List<int> ids);
        public Task<IEnumerable<CalendarEvent>> GetFixture(int id);
        public Task<IEnumerable<CalendarEvent>> GetFixtures(ICollection<int> ids);
        public Task<IEnumerable<CalendarEvent>> GetPlayoff(int id);
        public Task<IEnumerable<CalendarEvent>> GetPlayoffs(ICollection<int> ids);
    }
}
