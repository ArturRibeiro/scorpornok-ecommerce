using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;

namespace Frameworker.Integration.Tests.HttpClientExtensions
{
    public static class HttpClientExtensions
    {
        public static Result<IList<T>> GetList<T>(this HttpResponseMessage response)
            where T : class
        {
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            return JsonConvert.DeserializeObject<Result<IList<T>>>(result);
        }
    }
}