using System.Windows.Input;

namespace TourPlanner.UI.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;
        public event EventHandler? CanExecuteChanged;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
