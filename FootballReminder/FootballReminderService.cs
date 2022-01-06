using System;
using System.Collections.Generic;
using System.Linq;
using FootballReminder.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using FootballReminder.Services;
using System.Threading.Tasks;

namespace FootballReminder
{
    public class FootballReminderService : IFootballReminderService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FootballReminderService> _logger;
        private readonly IFootballDataProvider _data;
        private readonly ICalendarService _calendar;

        public FootballReminderService(IConfiguration config, ILogger<FootballReminderService> logger, IFootballDataProvider data, ICalendarService calendar)
        {
            _config = config;
            _logger = logger;
            _data = data;
            _calendar = calendar;
        }

        public async Task Run()
        {
            _logger.LogInformation("Program started.");

            var teams = _config.GetSection("TeamList").Get<TeamList>();

            var matches = new List<CalendarEvent>();

            try
            {
                foreach (List<int> ids in teams.HeadToHead)
                {
                    matches.AddRange(await _data.GetHeadToHeads(ids));
                }
                _logger.LogInformation("HeadToHeads finished.");
                matches.AddRange(await _data.GetFixtures(new List<int>(teams.Fixture)));
                _logger.LogInformation("Fixtures finished.");
                matches.AddRange(await _data.GetPlayoffs(new List<int>(teams.Playoff)));
                _logger.LogInformation("Playoffs finished.");
            }
            catch (Exception e)
            {
                _logger.LogError($"FootballDataError: {e}");
                return;
            }

            matches = matches
                .GroupBy(m => m.ExtraData["MatchId"])
                .Select(g => g.First())
                .ToList();
            
            IEnumerable<CalendarEvent> events = _calendar.GetEvents();

            var existingMatches = new List<CalendarEvent>();
            var newMatches = new List<CalendarEvent>();
            foreach (CalendarEvent m in matches)
            {
                CalendarEvent e = events.Where(e => e.ExtraData["MatchId"] == m.ExtraData["MatchId"])
                    .FirstOrDefault();
                if (e == null)
                {
                    newMatches.Add(m);
                }
                else
                {
                    m.ExtraData.Add("EventId", e.ExtraData["EventId"]);
                    existingMatches.Add(m);
                }
            }

            try
            {
                _calendar.AddEvents(newMatches);
                _calendar.UpdateEvents(existingMatches);
            }
            catch (Exception e)
            {
                _logger.LogError($"CalendarError: {e}");
                return;
            }

            _logger.LogInformation("Pushed to calendar.");
        }
    }
}
