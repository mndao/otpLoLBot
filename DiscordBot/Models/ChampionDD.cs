using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class ChampionDD
    {
        public string name { get; set; }
        public List<string> tags { get; set; }
        public List<string> allytips { get; set; }
        public List<string> enemytips { get; set; }
    }
}
