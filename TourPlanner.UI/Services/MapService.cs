using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class MapService : IMapService
    {
        private WebView2? _webView;

        public bool IsInitialized => _webView != null && _webView.CoreWebView2 != null;

        public void RegisterWebView(WebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView), "WebView cannot be null");
        }

        public Task InitializeAsync()
        {
            if (_webView == null)
            {
                throw new InvalidOperationException("WebView must be registered before initialization");
            }

            DisplayNoTourSelectedMessage();
            return Task.CompletedTask;
        }

        public async Task LoadGeoJsonMapAsync(string geoJson)
        {
            if (!IsInitialized || _webView?.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 is not initialized");
            }

            var htmlTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views", "Leaflet.html");
            if (!File.Exists(htmlTemplatePath))
            {
                _webView.CoreWebView2.NavigateToString("<html><body><h2 style='color:red;'>Leaflet.html not found!</h2></body></html>");
                return;
            }

            var html = await File.ReadAllTextAsync(htmlTemplatePath);
            html = html.Replace("__GEOJSON__", geoJson);
            _webView.CoreWebView2.NavigateToString(html);
        }

        public Task ClearMapAsync()
        {
            if (!IsInitialized || _webView?.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 is not initialized");
            }

            DisplayNoTourSelectedMessage();

            return Task.CompletedTask;
        }

        public async Task<MemoryStream> CaptureMapScreenshotAsync()
        {
            if (!IsInitialized || _webView?.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 is not initialized");
            }

            var screenshotStream = new MemoryStream();

            // Capture the screenshot of the WebView2 control
            await _webView.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, screenshotStream);

            // Reset the position of the memory stream to the beginning
            screenshotStream.Position = 0;

            return screenshotStream;
        }

        private void DisplayNoTourSelectedMessage()
        {
            if (_webView == null || _webView.CoreWebView2 == null)
            {
                throw new InvalidOperationException("WebView2 is not initialized");
            }

            _webView.CoreWebView2.NavigateToString(
                "<html><body style='background-color:#1F1F1F;'>" +
                "<div style='text-align:center; margin-top:50px; font-family:Arial; color:darkred'>" +
                "<h3>No tour selected</h3>" +
                "<p>Select or create a tour to view the map</p>" +
                "</div></body></html>");
        }
    }
}
