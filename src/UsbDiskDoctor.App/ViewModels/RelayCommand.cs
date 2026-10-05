using System.Windows.Input;

namespace UsbDiskDoctor.App.ViewModels
{
    /// <summary>
    /// A simple ICommand implementation that relays Execute and 
    /// CanExecute to delegates. Manual implementation without MVVM frameworks.
    /// </summary>
    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// Creates a new relay command.
        /// </summary>
        /// <param name="execute">The action to execute. Must not be null.</param>
        /// <param name="canExecute">Optional predicate; null means always executable.</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Forces WPF to re-evaluate CanExecute for commands bound to this instance.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}