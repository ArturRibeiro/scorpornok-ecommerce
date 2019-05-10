using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Helpers
{
    public abstract class BaseHttpServiceClient
    {
        private readonly HttpClient _client;
        private readonly int _port;

        public BaseHttpServiceClient(TestServer client, int port)
        {
            this._client = client.CreateClient();
            this._client.DefaultRequestHeaders.Accept.Clear();
            this._client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            this._port = port;
        }

        internal void AddHeader(string name, object value)
        {
            _client.DefaultRequestHeaders.Add(name, value.ToString());
        }

        public async Task<HttpResponseMessage> PostAsync(object messageRequest, string route)
            => await _client
            .PostAsync($"http://localhost:{this._port}/api/v1/{ route }", CreateStringContent(messageRequest));

        public async Task<HttpResponseMessage> GetAsync(string route)
            => await _client
            .SendAsync(CreateRequestMessage(HttpMethod.Get, $"http://localhost:{this._port}/api/v1/{route}"));

        private static StringContent CreateStringContent(object messageRequest)
            => new StringContent(JsonConvert.SerializeObject(messageRequest), Encoding.UTF8, "application/json");

        private static HttpRequestMessage CreateRequestMessage(HttpMethod method, string route)
            => new HttpRequestMessage { Method = method, RequestUri = new Uri(route) };
    }
}
