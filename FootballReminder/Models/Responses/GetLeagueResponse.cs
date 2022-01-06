using System.Collections.Generic;
using FootballReminder.Models.ApiFootballModels;
using Newtonsoft.Json;

namespace FootballReminder.Models.Responses
{
    public class GetLeagueResponse
    {
        [JsonProperty("response")]
        public List<LeagueResponse> LeagueResponse { get; set; }
    }

    public class LeagueResponse
    {
        public League League { get; set; }
        public Country Country { get; set; }
        public List<Season> Seasons { get; set; }
    }
}