using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Models;
using MingweiSamuel.Camille.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace DiscordBot.Services
{
    public class DataDragonService
    {
        private string PATCH = Environment.GetEnvironmentVariable("PATCH");
        private RestClient restClient;
        
        public DataDragonService()
        {
            restClient = new RestClient("http://ddragon.leagueoflegends.com/cdn/" + PATCH + "/");

        }

        public string GetChampionImageURL(string name)
        {
            return "http://ddragon.leagueoflegends.com/cdn/img/champion/splash/" + name + "_0.jpg";
        }

        public string GetMapURL(string name)
        {
            return "http://ddragon.leagueoflegends.com/cdn/6.8.1/img/map/map" + name + ".png";
        }

        public string GetSummonerIconURL(string name)
        {
            return "http://ddragon.leagueoflegends.com/cdn/" + PATCH + "/img/profileicon/" + name + ".png";
        }

        public string GetChampionIconURL(string name)
        {
            return "http://ddragon.leagueoflegends.com/cdn/" + PATCH + "/img/champion/" + name + ".png";
        }

        public async Task<ChampionDD> GetChampionInfoAsync(string name)
        {
            if(!Enum.IsDefined(typeof(Champion),name.ToUpper()))
            {
                throw new ArgumentException("Champion does not exist");
            }
            var requestName = name.Replace("_", "");
            var requestName2 = char.ToUpper(requestName[0]) + requestName.Substring(1);
            RestRequest restRequest = new RestRequest("data/en_US/champion/" + requestName2 + ".json", Method.GET);
            IRestResponse response = await restClient.ExecuteAsync(restRequest);
            var content = JObject.Parse(response.Content);
            var champData = (JObject)content["data"];
            var champData2 = (JObject)champData[requestName2];
            var tags = champData2["tags"].ToObject<List<string>>();
            var allyTips = champData2["allytips"].ToObject<List<string>>();
            var enemyTips =champData2["enemytips"].ToObject<List<string>>();

            var champDD = new ChampionDD
            {
                name = requestName2,
                allytips = allyTips,
                enemytips = enemyTips,
                tags = tags
            };
            return champDD;
        }

    }
}
