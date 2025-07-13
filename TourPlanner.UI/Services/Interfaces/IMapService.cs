using System.IO;
using Microsoft.Web.WebView2.Wpf;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface IMapService
    {
        public void RegisterWebView(WebView2 webView);
        Task InitializeAsync();
        Task LoadGeoJsonMapAsync(string geoJson);
        Task ClearMapAsync();
        Task<MemoryStream> CaptureMapScreenshotAsync();
        bool IsInitialized { get; }
    }
}
