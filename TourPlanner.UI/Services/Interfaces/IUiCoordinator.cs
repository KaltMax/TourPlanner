namespace TourPlanner.UI.Services.Interfaces
{
    public interface IUiCoordinator
    {
        event Action? RequestOpenAddTourView;
        event Action? RequestCloseAddTourView;

        event Action? RequestOpenAddTourLogView;
        event Action? RequestCloseAddTourLogView;

        event Action? RequestOpenEditTourView;
        event Action? RequestCloseEditTourView;

        event Action? RequestOpenEditTourLogView;
        event Action? RequestCloseEditTourLogView;

        void OpenAddTourView();
        void CloseAddTourView();

        void OpenAddTourLogView();
        void CloseAddTourLogView();

        void OpenEditTourView();
        void CloseEditTourView();

        void OpenEditTourLogView();
        void CloseEditTourLogView();

    }
}
