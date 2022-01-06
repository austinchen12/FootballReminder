using System.Collections.Generic;

namespace FootballReminder.Models
{
    public class TeamList
    {
        public List<List<int>> HeadToHead { get; set; }
        public List<int> Fixture { get; set; }
        public List<int> Playoff { get; set; }
    }
}
