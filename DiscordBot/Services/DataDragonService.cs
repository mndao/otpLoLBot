using System;
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

        public string GetSummonerIconURL(string name)
        {
            return "http://ddragon.leagueoflegends.com/cdn/" + PATCH + "/img/profileicon/" + name + ".png";
        }

    }
}
