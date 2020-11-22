using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks; 
using Discord;
using Discord.Commands;
using DiscordBot.Services; 

namespace DiscordBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public PictureService PictureService { get; set;}

        public RiotApiService RiotApiService { get; set;}
 

        [Command("GetRankedHistory")]
        public async Task GetRankedInfoAsync(params string[] objects)
        { 
            if (ValidRankedInfo(objects))
            {
                var result = await RiotApiService.GetRankedHistory(objects[0], objects[1], Int32.Parse(objects[2]));
                var embed = new EmbedBuilder
                {
                    Title = "Ranked History for " + objects[0],
                    Color = Color.Green,
                    Description = result
                };
                embed.WithCurrentTimestamp();
                await ReplyAsync(embed : embed.Build());

            }
            else
            {
                string objectsString = "";

                objects.ToList().ForEach(i => objectsString += i.ToString() + " ");


                var embed = new EmbedBuilder
                {
                    Title = "Issue with Command : " + objectsString,
                    Description = "Arguments should be the following:\n1: Summoner Name\n" +
                    "2: Reigon\n" +
                    "3: Number of Ranked Games (10 max)",
                    Color = Color.Red,
                };
                embed.WithCurrentTimestamp();
                await ReplyAsync(embed: embed.Build());
            }
        }

        private Boolean ValidRankedInfo(string[] objects)
        {
            int numberOfRankedGames;
            if (objects.Length != 3) return false;
            if (!int.TryParse(objects[2], out numberOfRankedGames)) return false;
            if (numberOfRankedGames > 10) return false;
            if (!RiotApiService.ValidReigon(objects[1])) return false; 

            return true; 
        }
    }
}
