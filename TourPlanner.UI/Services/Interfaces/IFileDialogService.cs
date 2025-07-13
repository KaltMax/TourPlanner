namespace TourPlanner.UI.Services.Interfaces
{
    public interface IFileDialogService
    {
        Task<string?> SaveFileDialogAsync(string defaultFileName, string filter = "All files (*.*)|*.*", string defaultExtension = "");
        Task<string?> OpenFileDialogAsync(string title, string filter);
    }
}