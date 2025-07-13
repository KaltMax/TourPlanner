using System.Net.Http;
using System.Text;
using System.Text.Json;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient _httpClient;

        public HttpService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            var response = await _httpClient.GetAsync(url);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<T?> PostAsync<T>(string url, T body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<T?> PutAsync<T>(string url, T body)
        {
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, content);
            return await HandleResponseAsync<T>(response);
        }

        public async Task DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            await HandleResponseAsync<object>(response);
        }

        public async Task<string> GetRawJsonAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(!string.IsNullOrEmpty(errorContent)
                    ? errorContent
                    : $"Request failed: {(int)response.StatusCode} ({response.ReasonPhrase})");
            }
        }

        public async Task<string> PostRawJsonAsync(string url, string jsonContent)
        {
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(!string.IsNullOrEmpty(errorContent)
                    ? errorContent
                    : $"Request failed: {(int)response.StatusCode} ({response.ReasonPhrase})");
            }
        }

        private async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return default;

                var responseJson = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseJson))
                    return default;

                return JsonSerializer.Deserialize<T>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                // Just get the error content directly
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(!string.IsNullOrEmpty(errorContent)
                    ? errorContent
                    : $"Request failed: {(int)response.StatusCode} ({response.ReasonPhrase})");
            }
        }
    }
}