using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Services;
using System.Linq;
using MingweiSamuel.Camille.SpectatorV4;
using MingweiSamuel.Camille.Enums;

namespace DiscordBot.Modules
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        public RiotApiService RiotApiService { get; set; }

        public DataDragonService DataDragonService { get; set; }

        private EmbedAuthorBuilder author = new EmbedAuthorBuilder
        {
            Name = "very-cool-bot", 
            IconUrl = "http://ddragon.leagueoflegends.com/cdn/" + Environment.GetEnvironmentVariable("PATCH") + "/img/champion/Sona.png"
        };

        //Usage : @botname GetItemInfo itemName
        [Command("GetItemInfo")]
        public async Task GetItemInfo(params string[] objects)
        {
            StringBuilder stringBuilder = new StringBuilder(); 
            for (int i = 0; i <= objects.Length - 1; i++)
            {
                stringBuilder.Append(objects[i] + " ");
            }
            string item = stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);
            var result = DataDragonService.GetItemByName(item);
            var listOfItems = DataDragonService.GetUpgradeItems(result.BuildsInto);
            var imageURL = DataDragonService.GetItemIconURL(result.Id);
            var embed = new EmbedBuilder
            {
                Title = item,
                Color = Color.Green
            };
            embed.WithAuthor(author);       
            
            if(!String.IsNullOrEmpty(result.Plaintext))
            {
                embed.AddField("Description", result.Plaintext); 

            }
            embed.AddField("Tags", string.Join(", ", result.Tags),true);
            embed.AddField("Cost", result.Cost,true);
            embed.AddField("Stats", result.Stats);
            if(listOfItems.Count > 0)
            {
                embed.AddField("Builds Into", string.Join("\n", listOfItems));
            }
            embed.ThumbnailUrl = imageURL;
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());
        }

        //Usage : @botname GetChampInfo championName
        [Command("GetChampInfo")]
        public async Task GetChampInfoAsync(params string[] objects)
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i <= objects.Length-1; i++)
            {
                stringBuilder.Append(objects[i] + "_");
            }
            string summonerName = stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);
            var result = await DataDragonService.GetChampionInfoAsync(summonerName);
            var embed = new EmbedBuilder
            {
                Title = result.name,
                Color = Color.Green
            };
            embed.WithAuthor(author);
            embed.AddField("Roles", string.Join(", ", result.tags));
            embed.AddField("Enemy Tips", string.Join("\n", result.enemytips));
            embed.AddField("Ally Tips", string.Join("\n", result.allytips));
            embed.ThumbnailUrl = DataDragonService.GetChampionIconURL(result.name);
            embed.ImageUrl = DataDragonService.GetChampionImageURL(result.name);
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());   
        }

        //Usage : @botname GetLiveMatchData summonerName Region 
        [Command("GetLiveMatchData")]
        public async Task GetLiveMatchDataAsync(params string[] objects)
        { 
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i <= objects.Length - 2; i++)
            {
                stringBuilder.Append(objects[i] + " ");
            }
            string summonerName = stringBuilder.ToString().Substring(0,stringBuilder.ToString().Length-1);
            string reigon = objects[objects.Length - 1]; 

            if(RiotApiService.ValidReigon(reigon) == false)
            {
                throw new ArgumentException("Arguments should be the following:\n1: Summoner Name\n" +
                    "2: Reigon\n");
           }
            await ReplyAsync("Obtaining summoner info for summoner : " + summonerName + " ...");
            CurrentGameInfo currentGameInfo = await RiotApiService.GetLiveMatchDataAsync(summonerName, reigon);
            var blueSide = currentGameInfo.Participants.Where(player => player.TeamId == 100);
            var redSide = currentGameInfo.Participants.Where(player => player.TeamId == 200);
            var summonerIcon = currentGameInfo.Participants.First(player => player.SummonerName == summonerName).ProfileIconId;
            StringBuilder blueStringBuilder = new StringBuilder();
            foreach (var participant in blueSide)
            {
                blueStringBuilder.AppendLine(participant.SummonerName + " : " + RiotApiService.MakeBold(((Champion)participant.ChampionId).Name()));
            }
            StringBuilder redStringBuilder = new StringBuilder();
            foreach (var participant in redSide)
            {
                redStringBuilder.AppendLine(participant.SummonerName + " : " + RiotApiService.MakeBold(((Champion)participant.ChampionId).Name()));

            }
            var embed = new EmbedBuilder
            {
                Title = $"Live Match Data for {summonerName}",
                Color = Color.Green,
                ThumbnailUrl = DataDragonService.GetMapURL(currentGameInfo.MapId.ToString()),
            };
            embed.AddField("Game Type", currentGameInfo.GameMode);
            embed.AddField("Blue Side", blueStringBuilder.ToString(), true);
            embed.AddField("Red Side", redStringBuilder.ToString(), true);
            embed.WithCurrentTimestamp();
            await ReplyAsync(embed: embed.Build());
        }

        //Usage: @botname GetRankedHistory summonerName Region NumGames
        [Command("GetRankedHistory")]
        public async Task GetRankedInfoAsync(params string[] objects)
        {
            ValidateRankedInfo(objects);
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i <= objects.Length - 3; i++)
            {
                stringBuilder.Append(objects[i] + " ");
            }

            string summonerName = stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);

            await ReplyAsync("Obtaining ranked stats for summoner :" + summonerName + " ...");

            var result = await RiotApiService.GetRankedHistoryAsync(summonerName, objects[objects.Length - 2], Int32.Parse(objects[objects.Length - 1]));
            var embed = new EmbedBuilder
            {
                Title = "Ranked History for " + objects[0],
                Color = Color.Green,
                ThumbnailUrl = DataDragonService.GetSummonerIconURL(result["IconID"]),
            };
            embed.WithAuthor(author);
            embed.AddField("Wins", result["Wins"], true);
            embed.AddField("Losses", result["Loss"], true);
            embed.AddField("Win rate", result["Winrate"], true);
            embed.AddField("Match History", result["Data"], false);

            embed.WithCurrentTimestamp();
            await ReplyAsync(embed: embed.Build());
        }

        //Usage : @botname SearchSummoner summonerName region 
        [Command("SearchSummoner")]
        public async Task GetSummonerInfoAsync(params string[] objects)
        {
            if (RiotApiService.ValidReigon(objects[objects.Length - 1]) == false)
            {
                throw new ArgumentException("Arguments should be the following:\n1: Summoner Name\n" +
                    "2: Reigon\n");
            }
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i <= objects.Length - 2; i++)
            {
                stringBuilder.Append(objects[i] + " ");

            }
            string summonerName = stringBuilder.ToString().Substring(0, stringBuilder.ToString().Length - 1);

            await ReplyAsync("Obtaining summoner info for summoner : " + summonerName + " ...");

            var result = await RiotApiService.GetSummonerInfoAsync(summonerName, objects[objects.Length - 1]);

            var embed = new EmbedBuilder();
            embed.WithAuthor(author);
            embed.Color = Color.Green;
            embed.Title = summonerName;
            embed.AddField("Level", result["Level"]);
            if (result.ContainsKey("RANKED_FLEX_SR"))
            {
                embed.AddField("Flex Rank", GetRankedLogo(result["RANKED_FLEX_SR"]) + " " + result["RANKED_FLEX_SR"], true);
                embed.AddField("Flex WR", result["RANKED_FLEX_SRWR"], true);
            }
            if (result.ContainsKey("RANKED_SOLO_5x5"))
            {
                embed.AddField("Solo Rank", GetRankedLogo(result["RANKED_SOLO_5x5"]) + " " + result["RANKED_SOLO_5x5"], true);
                embed.AddField("Solo WR", result["RANKED_SOLO_5x5WR"], true);
            }
            embed.AddField("Highest Champion Mastery",
                result["ChampMastery"] + " " + result["ChampMasteryScore"] + " points");
            embed.ImageUrl = DataDragonService.GetChampionImageURL(result["ChampMastery"]);
            embed.ThumbnailUrl = DataDragonService.GetSummonerIconURL(result["IconID"]);
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());     
        }

        private string GetRankedLogo(string rank)
        {
            var rankName = rank.Split(" ")[0];
            var color = rankName switch
            {
                "IRON" => "<:Iron:793016881237327873>",
                "BRONZE" => "<:Bronze:793014741039185920>",
                "SILVER" => "<:Silver:793016882801672202>",
                "GOLD" => "<:Gold:793016882944016415>",
                "PLATINUM" => "<:Platinum:793016882202279957>",
                "DIAMOND" => "<:Diamond:793016883871219732>",
                "MASTER" => "<:Master:793016881409425418>",
                "GRANDMASTER" => "<:Grandmaster:793016881501175808>",
                "CHALLENGER" => "<:Challenger:793011383259234304>",
                _ => "",
            };
            return color;
        }

        private void ValidateRankedInfo(string[] objects)
        {
            bool valid = true; 
            int arrayLength = objects.Length; 
            int numberOfRankedGames;
            if (!int.TryParse(objects[arrayLength-1], out numberOfRankedGames)) valid = false;
            if (numberOfRankedGames > 10) valid = false;
            if (!RiotApiService.ValidReigon(objects[arrayLength-2])) valid = false;

            if(!valid)
            {
                throw new ArgumentException("Arguments should be the following:\n1: Summoner Name\n" +
                    "2: Reigon\n" +
                    "3: Number of Ranked Games (10 max)");
            }
        }
    }
}