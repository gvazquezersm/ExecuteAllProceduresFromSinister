using System;
using ExecuteAllProceduresFromSinister.Common;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ExecuteAllProceduresFromSinister.Core.HttpClient
{
    public static class BaseHttpClientService       
    {
        private static System.Net.Http.HttpClient CreateHttpClient(string baseUrl)
        {
            // Leer la variable de entorno BASE_URL si está definida (local.settings.json -> Values -> "BASE_URL")
            var effectiveBaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? baseUrl;

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            var httpClient = new System.Net.Http.HttpClient(handler)
            {
                BaseAddress = new System.Uri(effectiveBaseUrl)
            };
            httpClient.DefaultRequestHeaders.Add(CommonConstants.BasicAuth, CommonConstants.BasicValue);

            return httpClient;
        }

        private static StringContent LoadJsonContent<TEntity>(TEntity entity)
        {
            string jsonData = JsonConvert.SerializeObject(entity);
            StringContent res = new StringContent(jsonData, Encoding.UTF8, CommonConstants.MediaType);
            return res;
        }

        public async static Task<TResult> PostAsync<TRequest, TResult>(string endpoint, TRequest dataRequest)
        {
            TResult result = default;
            var client = CreateHttpClient(CommonConstants.BaseUrl);
            var response = await client.PostAsync(endpoint, LoadJsonContent(dataRequest));
            if (response != null && response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<TResult>();
            }

            return result;
        }

        public static async Task<TResult> GetAsync<TResult>(string endpoint)
        {
            TResult result = default;
            var client = CreateHttpClient(CommonConstants.BaseUrl);
            var response = await client.GetAsync(endpoint);
            if (response.IsSuccessStatusCode)
            {
                result = await response.Content.ReadAsAsync<TResult>();
            }

            return result;
        }
    }
}
