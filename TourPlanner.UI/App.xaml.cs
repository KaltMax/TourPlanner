using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TourPlanner.UI.Services;
using TourPlanner.UI.Services.Interfaces;
using TourPlanner.UI.ViewModels;

namespace TourPlanner.UI
{
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IHttpService, HttpService>();
            services.AddSingleton<ITourDataService, TourDataService>();
            services.AddSingleton<ISelectedTourService, SelectedTourService>();
            services.AddSingleton<IUiCoordinator, UiCoordinator>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IMapService, MapService>();
            services.AddSingleton<ITourReportPdfService, TourReportPdfService>();
            services.AddSingleton<ITourSummaryPdfService, TourSummaryPdfService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IApplicationService, ApplicationService>();

            // Register ViewModels
            services.AddSingleton<TourListViewModel>();
            services.AddSingleton<TourLogsViewModel>();
            services.AddSingleton<TourInfoViewModel>();
            services.AddSingleton<MenuBarViewModel>();
            services.AddSingleton<SearchViewModel>();
            services.AddSingleton<MainWindowViewModel>();

            // Register MainWindow
            services.AddSingleton<MainWindow>();

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Load data before showing window
            var dataService = _serviceProvider.GetRequiredService<ITourDataService>();
            await dataService.LoadToursAsync();

            // Set ViewModel + show MainWindow
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            mainWindow.Show();
        }
    }
}
