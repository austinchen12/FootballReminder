using System.Collections.Generic;

namespace FootballReminder.Models
{
    public class TeamList
    {
        public List<List<int>> HeadToHead { get; set; } = new List<List<int>>();
        public List<int> Fixture { get; set; } = new List<int>();
        public List<int> Playoff { get; set; } = new List<int>();
    }
}
