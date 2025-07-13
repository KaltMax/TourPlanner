using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class UiCoordinator : IUiCoordinator
    {
        public event Action? RequestOpenAddTourView;
        public event Action? RequestCloseAddTourView;

        public event Action? RequestOpenAddTourLogView;
        public event Action? RequestCloseAddTourLogView;

        public event Action? RequestOpenEditTourView;
        public event Action? RequestCloseEditTourView;

        public event Action? RequestOpenEditTourLogView;
        public event Action? RequestCloseEditTourLogView;

        public void OpenAddTourView() => RequestOpenAddTourView?.Invoke();
        public void CloseAddTourView() => RequestCloseAddTourView?.Invoke();

        public void OpenAddTourLogView() => RequestOpenAddTourLogView?.Invoke();
        public void CloseAddTourLogView() => RequestCloseAddTourLogView?.Invoke();

        public void OpenEditTourView() => RequestOpenEditTourView?.Invoke();
        public void CloseEditTourView() => RequestCloseEditTourView?.Invoke();

        public void OpenEditTourLogView() => RequestOpenEditTourLogView?.Invoke();
        public void CloseEditTourLogView() => RequestCloseEditTourLogView?.Invoke();
    }
}
