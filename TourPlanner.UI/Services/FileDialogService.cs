using Microsoft.Win32;
using TourPlanner.UI.Services.Interfaces;

namespace TourPlanner.UI.Services
{
    public class FileDialogService : IFileDialogService
    {
        public Task<string?> SaveFileDialogAsync(string defaultFileName, string filter = "All files (*.*)|*.*", string defaultExtension = "")
        {
            return Task.Run(() =>
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = defaultFileName,
                    Filter = filter,
                    DefaultExt = defaultExtension,
                    AddExtension = true
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    return saveFileDialog.FileName;
                }

                return null;
            });
        }

        public Task<string?> OpenFileDialogAsync(string title, string filter)
        {
            return Task.Run(() =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = title,
                    Filter = filter
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    return openFileDialog.FileName;
                }

                return null;
            });
        }
    }
}