using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System.IO;
using System.Reflection;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class MapService : IMapService
    {
        private const string WebViewNotInitializedMessage = "WebView2 is not initialized";
        private const string OsmTileFilter = "https://tile.openstreetmap.org/*";
        private static readonly string AppUserAgent =
            $"TourPlanner/{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0"}";

        private WebView2? _webView;
        private CoreWebView2? _configuredCore;

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

            ConfigureTileRequests();
            DisplayNoTourSelectedMessage();
            return Task.CompletedTask;
        }

        private void ConfigureTileRequests()
        {
            var core = _webView?.CoreWebView2;

            // Initialization can run more than once (e.g. repeated Loaded events) - only subscribe once per core
            if (core == null || ReferenceEquals(core, _configuredCore))
            {
                return;
            }

            core.AddWebResourceRequestedFilter(OsmTileFilter, CoreWebView2WebResourceContext.Image);
            core.WebResourceRequested += OnWebResourceRequested;
            _configuredCore = core;
        }

        private static void OnWebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            e.Request.Headers.SetHeader("User-Agent", AppUserAgent);
        }

        public async Task LoadGeoJsonMapAsync(string geoJson)
        {
            if (!IsInitialized || _webView?.CoreWebView2 == null)
            {
                throw new InvalidOperationException(WebViewNotInitializedMessage);
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
                throw new InvalidOperationException(WebViewNotInitializedMessage);
            }

            DisplayNoTourSelectedMessage();

            return Task.CompletedTask;
        }

        public async Task<MemoryStream> CaptureMapScreenshotAsync()
        {
            if (!IsInitialized || _webView?.CoreWebView2 == null)
            {
                throw new InvalidOperationException(WebViewNotInitializedMessage);
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
                throw new InvalidOperationException(WebViewNotInitializedMessage);
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
