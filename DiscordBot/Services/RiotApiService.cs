using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Exceptions;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.MatchV4;
using MingweiSamuel.Camille.SpectatorV4;

namespace DiscordBot.Services
{ 
    public class RiotApiService
    {
        private readonly RiotApi riotApi;
        public RiotApiService()
        {
            riotApi = RiotApi.NewInstance(Environment.GetEnvironmentVariable("RIOTAPI"));
        }

        public async Task<CurrentGameInfo> GetLiveMatchDataAsync(string summonerName, string reigon)
        {
            var summonerData = await riotApi.SummonerV4.GetBySummonerNameAsync(Region.Get(reigon), summonerName);
            if(summonerData == null)
            {
                await Task.Run(() => throw new RiotApiException($"Summoner '{summonerName} not found."));
            }
            CurrentGameInfo currentGameInfo = await riotApi.SpectatorV4.GetCurrentGameInfoBySummonerAsync(Region.Get(reigon), summonerData.Id);

            if(currentGameInfo == null)
            {
                await Task.Run(() => throw new RiotApiException($"Summoner '{summonerName} is not currently in a game."));
            }
            return currentGameInfo;
        }

        public async Task<Dictionary<string,string>> GetSummonerInfoAsync(string summonerName, string reigon)
        {
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            var summonerData = await riotApi.SummonerV4.GetBySummonerNameAsync(Region.Get(reigon), summonerName);
            if(summonerData == null)
            {
                await Task.Run(() => throw new RiotApiException($"Summoner '{summonerName}' not found."));
            }
            var rankedData = await riotApi.LeagueV4.GetLeagueEntriesForSummonerAsync(Region.Get(reigon), summonerData.Id);
            var masteryData = await riotApi.ChampionMasteryV4.GetAllChampionMasteriesAsync(Region.Get(reigon), summonerData.Id);

            foreach (var data in rankedData)
            {
                keyValuePairs.Add(data.QueueType, data.Tier + " " + data.Rank);
                var winRate = CalculateWinRate(data.Wins, data.Losses); 
                keyValuePairs.Add(data.QueueType + "WR", winRate.ToString() + "%"); 
            }

            keyValuePairs.Add("IconID", summonerData.ProfileIconId.ToString());
            keyValuePairs.Add("Name", summonerName);
            keyValuePairs.Add("Level", summonerData.SummonerLevel.ToString());
            keyValuePairs.Add("ChampMastery",((Champion)masteryData[0].ChampionId).Name());
            keyValuePairs.Add("ChampMasteryScore", masteryData[0].ChampionPoints.ToString());

            return keyValuePairs; 
        }

        public async Task<Dictionary<string,string>> GetRankedHistoryAsync(string summonerName,string reigon, int numGames)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(); 
            StringBuilder stringBuilder = new StringBuilder();

            var summonerData = await riotApi.SummonerV4.GetBySummonerNameAsync(Region.Get(reigon), summonerName);

            if(summonerData == null)
            {
                await Task.Run(() => throw new RiotApiException($"Summoner '{summonerName}' not found."));
            }

            dictionary.Add("IconID", summonerData.ProfileIconId.ToString()); 

            var matchlist = await riotApi.MatchV4.GetMatchlistAsync(
                Region.Get(reigon), summonerData.AccountId, queue: new[] { 420 }, endIndex: numGames);

            var matchDataTasks = matchlist.Matches.Select(
                matchMetadata => riotApi.MatchV4.GetMatchAsync(Region.Get(reigon), matchMetadata.GameId)).ToArray();

            var matchDatas = await Task.WhenAll(matchDataTasks);

            var index = 0;
            var winCount = 0;

            foreach (Match match in matchDatas)
            {
                var participantIdData = match.ParticipantIdentities
                    .First(pi => summonerData.Id.Equals(pi.Player.SummonerId));

                var participant = match.Participants
                    .First(p => p.ParticipantId == participantIdData.ParticipantId);

                var win = participant.Stats.Win;
                var champ = (Champion)participant.ChampionId;
                var kills = participant.Stats.Kills;
                var deaths = participant.Stats.Deaths;
                var assist = participant.Stats.Assists;
                var kda = (kills + assist) / (float)deaths;

                if(win)
                {
                    winCount += 1; 
                }
                string dot = win ? ":green_circle: " : ":red_circle: "; 
                string line = dot + " " + kills + "/" + deaths + "/" + assist + " as " + MakeBold(champ.Name());

                stringBuilder.AppendLine(line);
                stringBuilder.AppendLine("");
                index += 1; 
            }
            dictionary.Add("Wins", winCount.ToString());
            dictionary.Add("Loss", (numGames - winCount).ToString());
            dictionary.Add("Winrate", CalculateWinRate(winCount,numGames-winCount).ToString() + "%");

            dictionary.Add("Data", stringBuilder.ToString());

            return dictionary; 
        }

        public bool ValidReigon(string reigon)
        {
            try
            {
                Region.Get(reigon);
                return true;
            }
            catch(Exception)
            {
                return false; 
            }
        }

        private int CalculateWinRate(int wins, int losses)
        {
            if (losses == 0) return 100; 
            float winrate = (float)wins / (float)(wins + losses) * 100;
            return (int)winrate;
        }

        public string MakeBold(string word)
        {
            return "**" + word + "**";
        }
    }
}
