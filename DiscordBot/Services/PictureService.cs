﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks; 

namespace DiscordBot.Services
{
    public class PictureService
    {
        private readonly HttpClient http;

        public PictureService(HttpClient http)
            => this.http = http;

        public async Task<Stream> GetPictureAsync()
        {
            var resp = await http.GetAsync("https://cataas.com/cat");
            return await resp.Content.ReadAsStreamAsync(); 
        }
    }
}
