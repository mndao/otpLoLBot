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
                blueStringBuilder.AppendLine(participant.SummonerName + " : " + ((Champion)participant.ChampionId).Name());
            }
            StringBuilder redStringBuilder = new StringBuilder();
            foreach (var participant in redSide)
            {
                redStringBuilder.AppendLine(participant.SummonerName + " : " + ((Champion)participant.ChampionId).Name());

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
                stringBuilder.Append(objects[i]);
            }

            await ReplyAsync("Obtaining ranked stats for summoner :" + stringBuilder.ToString() + " ...");

            var result = await RiotApiService.GetRankedHistoryAsync(stringBuilder.ToString(), objects[objects.Length - 2], Int32.Parse(objects[objects.Length - 1]));
            var embed = new EmbedBuilder
            {
                Title = "Ranked History for " + objects[0],
                Color = Color.Green,
                Description = result["Data"],
                ThumbnailUrl = DataDragonService.GetSummonerIconURL(result["IconID"]),
            };
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
                stringBuilder.Append(objects[i]);
            }
            string summonerName = stringBuilder.ToString();
            await ReplyAsync("Obtaining summoner info for summoner : " + summonerName + " ...");

            var result = await RiotApiService.GetSummonerInfoAsync(summonerName, objects[objects.Length - 1]);

            var embed = new EmbedBuilder();
            embed.Title = summonerName;
            embed.AddField("Level", result["Level"]);
            if (result.ContainsKey("RANKED_FLEX_SR"))
            {
                embed.AddField("Flex Rank", result["RANKED_FLEX_SR"], true);
                embed.AddField("Flex WR", result["RANKED_FLEX_SRWR"], true);
                embed.Color = RankedBorderColor(result["RANKED_FLEX_SR"]);
            }
            if (result.ContainsKey("RANKED_SOLO_5x5"))
            {
                embed.AddField("Solo Rank", result["RANKED_SOLO_5x5"], true);
                embed.AddField("Solo WR", result["RANKED_SOLO_5x5WR"], true);
                embed.Color = RankedBorderColor(result["RANKED_SOLO_5x5"]);

            }
            embed.AddField("Highest Champion Mastery",
                result["ChampMastery"] + " " + result["ChampMasteryScore"] + " points");
            embed.ImageUrl = DataDragonService.GetChampionImageURL(result["ChampMastery"]);
            embed.ThumbnailUrl = DataDragonService.GetSummonerIconURL(result["IconID"]);
            embed.WithCurrentTimestamp();

            await ReplyAsync(embed: embed.Build());     
        }

        private Color RankedBorderColor(string rank)
        {
            var rankName = rank.Split(" ")[0];
            var color = rankName switch
            {
                "GOLD" => Color.Gold,
                "PLATINUM" => Color.DarkGreen,
                "DIAMOND" => Color.Blue,
                "MASTER" => Color.Blue,
                "GRANDMASTER" => Color.Blue,
                "CHALLENGER" => Color.Blue,
                _ => Color.Default,
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