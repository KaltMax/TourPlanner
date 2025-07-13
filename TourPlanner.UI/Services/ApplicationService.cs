using System.Windows;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class ApplicationService : IApplicationService
    {
        public void Shutdown()
        {
            Application.Current.Shutdown();
        }
    }
}