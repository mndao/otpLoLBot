using System.Collections.Generic;

namespace DiscordBot.Models
{
    public class ItemDD
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Plaintext { get; set; }
        public string Cost { get; set;}
        public string Stats { get; set;}
        public List<string> BuildsInto { get; set; }
        public List<string> Tags { get; set; }
    }
}
