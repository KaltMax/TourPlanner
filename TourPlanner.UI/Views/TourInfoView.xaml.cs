using System.Windows.Controls;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.UI.Views
{
    public partial class TourInfoView : UserControl
    {
        public TourInfoView()
        {
            InitializeComponent();
            this.Loaded += TourInfoView_Loaded;
        }

        private async void TourInfoView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Only handle UI-specific initialization
            if (DataContext is TourInfoViewModel viewModel)
            {
                // Ensure WebView2 core is ready
                await MapView.EnsureCoreWebView2Async();
                
                // Delegate business logic to ViewModel
                await viewModel.InitializeMapAsync(MapView);
            }
        }
    }
}