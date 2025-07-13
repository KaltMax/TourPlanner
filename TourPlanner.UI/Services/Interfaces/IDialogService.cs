using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourPlanner.UI.Services.Interfaces
{
    public interface IDialogService
    {
        void ShowInformation(string message, string title);
        void ShowError(string message, string title);
        bool ShowConfirmation(string message, string title);
    }
}
