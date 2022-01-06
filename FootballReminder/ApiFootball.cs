using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FootballReminder.Models;
using FootballReminder.Models.Responses;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using FootballReminder.Services;
using FootballReminder.Models.ApiFootballModels;

namespace FootballReminder
{
    public class ApiFootball : IFootballDataProvider
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private const int KEY_LIMIT = 100;
        private int _count;

        public ApiFootball(IConfiguration config)
        {
            _config = config;
            _httpClient = createClient();
            _count = 0;
        }

        public async Task<IEnumerable<CalendarEvent>> GetHeadToHead(int firstTeamId, int secondTeamId)
        {
            var matches = new List<CalendarEvent>();

            var url = "fixtures/headtohead?" +
                $"h2h={firstTeamId}-{secondTeamId}&" +
                $"from={DateTime.Today:yyyy-MM-dd}&" +
                $"to={DateTime.Today.AddMonths(3):yyyy-MM-dd}&" +
                $"timezone=America/New_York";

            HttpResponseMessage response = await get(url);

            var headToHeadResponse = JsonConvert.DeserializeObject<GetFixtureResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            foreach (FixtureResponse fixture in headToHeadResponse.FixtureResponse)
            {
                var match = new CalendarEvent()
                {
                    Summary = $"{fixture.Teams.Home.Name} v. {fixture.Teams.Away.Name}",
                    Datetime = fixture.Fixture.Date,
                    Duration = TimeSpan.FromHours(2),
                    Location = $"{fixture.Fixture.Venue.Name}, {fixture.Fixture.Venue.City}",
                    Description = $"{fixture.League.Round}, {fixture.League.Name}",
                    ExtraData = new Dictionary<string, string>()
                    {
                        { "MatchId", fixture.Fixture.Id.Value.ToString() }
                    }
                };

                matches.Add(match);
            }

            return matches;
        }

        public async Task<IEnumerable<CalendarEvent>> GetHeadToHeads(List<int> teamIds)
        {
            var matches = new List<CalendarEvent>();

            for (int i = 0; i < teamIds.Count; i++)
            {
                for (int j = i + 1; j < teamIds.Count; j++)
                {
                    matches.AddRange(await GetHeadToHead(teamIds[i], teamIds[j]));
                }
            }

            return matches;
        }

        public async Task<IEnumerable<CalendarEvent>> GetFixture(int teamId)
        {
            var matches = new List<CalendarEvent>();

            string url = "fixtures/?" +
                $"team={teamId}&" +
                $"next={5}&" +
                $"timezone=America/New_York";

            HttpResponseMessage response = await get(url);
            var headToHeadResponse = JsonConvert.DeserializeObject<GetFixtureResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            foreach (FixtureResponse fixture in headToHeadResponse.FixtureResponse)
            {
                var match = new CalendarEvent()
                {
                    Summary = $"{fixture.Teams.Home.Name} v. {fixture.Teams.Away.Name}",
                    Datetime = fixture.Fixture.Date,
                    Duration = TimeSpan.FromHours(2),
                    Location = $"{fixture.Fixture.Venue.Name}, {fixture.Fixture.Venue.City}",
                    Description = $"{fixture.League.Round}, {fixture.League.Name}",
                    ExtraData = new Dictionary<string, string>()
                    {
                        { "MatchId", fixture.Fixture.Id.Value.ToString() }
                    }
                };

                matches.Add(match);
            }

            return matches;
        }

        public async Task<IEnumerable<CalendarEvent>> GetFixtures(ICollection<int> teamIds)
        {
            var matches = new List<CalendarEvent>();

            foreach (int id in teamIds)
            {
                matches.AddRange(await GetFixture(id));
            }

            return matches;
        }

        public async Task<IEnumerable<CalendarEvent>> GetPlayoff(int leagueId)
        {
            var matches = new List<CalendarEvent>();

            int season = await getCurrentSeason(leagueId);

            var url = "fixtures/?" +
                $"league={leagueId}&" +
                $"season={season}&" +
                $"from={DateTime.Today:yyyy-MM-dd}&" +
                $"to={DateTime.Today.AddMonths(3):yyyy-MM-dd}&" +
                $"timezone=America/New_York";

            HttpResponseMessage response = await get(url);
            var fixtureResponse = JsonConvert.DeserializeObject<GetFixtureResponse>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            foreach (FixtureResponse fixture in fixtureResponse.FixtureResponse)
            {
                if (fixture.League.Round.ToLower().Contains("final"))
                {
                    var match = new CalendarEvent()
                    {
                        Summary = $"{fixture.Teams.Home.Name} v. {fixture.Teams.Away.Name}",
                        Datetime = fixture.Fixture.Date,
                        Duration = TimeSpan.FromHours(2),
                        Location = $"{fixture.Fixture.Venue.Name}, {fixture.Fixture.Venue.City}",
                        Description = $"{fixture.League.Round}, {fixture.League.Name}",
                        ExtraData = new Dictionary<string, string>()
                        {
                            { "MatchId", fixture.Fixture.Id.Value.ToString() }
                        }
                    };

                    matches.Add(match);
                }
            }

            return matches;
        }

        public async Task<IEnumerable<CalendarEvent>> GetPlayoffs(ICollection<int> leagueIds)
        {
            var matches = new List<CalendarEvent>();

            foreach (int id in leagueIds)
            {
                matches.AddRange(await GetPlayoff(id));
            }

            return matches;
        }

        private HttpClient createClient()
        {
            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://api-football-v1.p.rapidapi.com/v3/")
            };

            return client;
        }

        public async Task<int> getCurrentSeason(int leagueId)
        {
            var url = "leagues/?" +
                $"id={leagueId}&" +
                $"current=true";

            HttpResponseMessage response = await get(url);

            var leagueResponse = JsonConvert.DeserializeObject<GetLeagueResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

            if (leagueResponse.LeagueResponse != null)
            {
                foreach (Season season in leagueResponse.LeagueResponse[0].Seasons)
                {
                    if (season.Current)
                        return season.Year;
                }
            }

            return 0;
        }

        private async Task<HttpResponseMessage> get(string url)
        {
            if (_count++ % KEY_LIMIT == 0)
            {
                _httpClient.DefaultRequestHeaders.Remove("x-rapidapi-key");
                _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _config.GetSection($"ApiFootballKeys:{_count / KEY_LIMIT}").Value);
            }

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            System.Threading.Thread.Sleep(2000);

            return response;
        }
    }
}
