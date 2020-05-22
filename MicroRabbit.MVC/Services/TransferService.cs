﻿using MicroRabbit.MVC.Models.DTO;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroRabbit.MVC.Services
{
    public class TransferService : ITransferService
    {
        private readonly HttpClient _apiClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public TransferService(HttpClient apiClient,
            IHttpClientFactory httpClientFactory)
        {
            _apiClient = apiClient;
            _httpClientFactory = httpClientFactory;
        }

        public async Task Transfer(TransferDTO transferDTO)
        {
            var uri = "https://localhost:5001/api/banking";
            var transferContent = new StringContent(JsonConvert.SerializeObject(transferDTO), Encoding.UTF8, "application/json");

            //var clientFactory = app.ApplicationServices.GetService<IHttpClientFactory>();
            using (var httpClient = _httpClientFactory.CreateClient("Tracer"))
            {
                var response = await httpClient.PostAsync(uri, transferContent);
                response.EnsureSuccessStatusCode();
            }


            //var response = await _apiClient.PostAsync(uri, transferContent);
            //response.EnsureSuccessStatusCode();
        }
    }
}
