using System.Collections.Generic;
using FootballReminder.Models.ApiFootballModels;
using Newtonsoft.Json;

namespace FootballReminder.Models.Responses
{
    public class GetFixtureResponse
    {
        [JsonProperty("response")]
        public List<FixtureResponse> FixtureResponse { get; set; }
    }

    public class FixtureResponse
    {
        public Fixture Fixture { get; set; }
        public League League { get; set; }
        public Teams Teams { get; set; }
        public Goals Goals { get; set; }
        public Score Score { get; set; }
    }
}
