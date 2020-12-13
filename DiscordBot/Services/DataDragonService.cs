using System;
using System.Threading.Tasks;
using MingweiSamuel.Camille.Enums;
using Newtonsoft.Json;
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

        public async Task GetChampionInfoAsync(string name)
        {
            if(!Enum.IsDefined(typeof(Champion),name.ToUpper()))
            {
                throw new ArgumentException("Champion does not exist");
            }
            RestRequest restRequest = new RestRequest("data/en_US/champion/" + name + ".json", Method.GET);
            IRestResponse response = await restClient.ExecuteAsync(restRequest);
            dynamic content = JsonConvert.DeserializeObject(response.Content);
            dynamic moreContent = content["data"];
        }

    }
}
