using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MingweiSamuel.Camille;
using MingweiSamuel.Camille.Enums;
using MingweiSamuel.Camille.MatchV4;

namespace DiscordBot.Services
{ 
    public class RiotApiService
    {
        private readonly RiotApi riotApi;

        public RiotApiService()
        {
            riotApi = RiotApi.NewInstance(Environment.GetEnvironmentVariable("RIOTAPI"));
        }

        public async Task<string> GetRankedHistory(string summonerName,string reigon, int numGames)
        {
            StringBuilder stringBuilder = new StringBuilder();

            var summonerData = await riotApi.SummonerV4.GetBySummonerNameAsync(Region.Get(reigon), summonerName);

            if(summonerData == null)
            {
                stringBuilder.AppendLine($"Summoner '{summonerName}' not found.");
                return stringBuilder.ToString(); 
            }

            var matchlist = await riotApi.MatchV4.GetMatchlistAsync(
                Region.Get(reigon), summonerData.AccountId, queue: new[] { 420 }, endIndex: numGames);

            var matchDataTasks = matchlist.Matches.Select(
                matchMetadata => riotApi.MatchV4.GetMatchAsync(Region.Get(reigon), matchMetadata.GameId)).ToArray();

            var matchDatas = await Task.WhenAll(matchDataTasks);

            var index = 0; 

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
                var assits = participant.Stats.Assists;
                var kda = (kills + assits) / (float)deaths;

                string result = win ? "Won" : "Lost";
                string line = result + " " + kills + "/" + deaths + "/" + assits + " as " + champ.Name();

                stringBuilder.AppendLine(line);
                stringBuilder.AppendLine("");
                index += 1; 
            }

            return stringBuilder.ToString(); 
        }

        public Boolean ValidReigon(string reigon)
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
    }
}
