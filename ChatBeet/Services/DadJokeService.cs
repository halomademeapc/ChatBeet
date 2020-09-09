﻿using ChatBeet.Models;
using ChatBeet.Utilities;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChatBeet.Services
{
    public class DadJokeService
    {
        private readonly HttpClient client;

        public DadJokeService(IHttpClientFactory clientFactory)
        {
            client = clientFactory.CreateClient();
        }

        public async Task<string> GetDadJokeAsync()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://icanhazdadjoke.com/")
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                using var contentStream = await response.Content.ReadAsStreamAsync();
                return contentStream.DeserializeJson<DadJoke>()?.Joke;
            }

            return null;
        }
    }
}
