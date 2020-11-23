using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Services;

namespace DiscordBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public PictureService PictureService { get; set; }

        public RiotApiService RiotApiService { get; set; }


        [Command("GetRankedHistory")]
        public async Task GetRankedInfoAsync(params string[] objects)
        {
            if (ValidRankedInfo(objects))
            {
                StringBuilder stringBuilder = new StringBuilder();

                for(int i = 0; i<= objects.Length-3; i++)
                {
                    stringBuilder.Append(objects[i]);
                }

                var result = await RiotApiService.GetRankedHistory(stringBuilder.ToString(), objects[objects.Length-2], Int32.Parse(objects[objects.Length-1]));
                var embed = new EmbedBuilder
                {
                    Title = "Ranked History for " + objects[0],
                    Color = Color.Green,
                    Description = result["Data"],
                    ThumbnailUrl = "http://ddragon.leagueoflegends.com/cdn/10.23.1/img/profileicon/" + result["IconID"] + ".png"
                };
                embed.WithCurrentTimestamp();
                await ReplyAsync(embed: embed.Build());

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
            int arrayLength = objects.Length; 
            int numberOfRankedGames;
            if (!int.TryParse(objects[arrayLength-1], out numberOfRankedGames)) return false;
            if (numberOfRankedGames > 10) return false;
            if (!RiotApiService.ValidReigon(objects[arrayLength-2])) return false;

            return true;
        }
    }
}