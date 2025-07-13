namespace TourPlanner.UI.Services.Interfaces
{
    public interface IHttpService
    {
        Task<T?> GetAsync<T>(string url);
        Task<T?> PostAsync<T>(string url, T body);
        Task<T?> PutAsync<T>(string url, T body);
        Task DeleteAsync(string url);
        Task<string> GetRawJsonAsync(string url);
        Task<string> PostRawJsonAsync(string url, string jsonContent);
    }
}
