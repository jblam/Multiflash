using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JBlam.Multiflash.Helpers
{
    static class Command
    {
        public static ParameterlessCommand Create(Action execute, Func<bool>? canExecute = null)
            => new ParameterlessCommand(execute, canExecute);

        public static ParametricCommand<T> Create<T>(Action<T?> execute, Func<T?, bool>? canExecute = null)
            => new ParametricCommand<T>(execute, canExecute);
    }

    class ParameterlessCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public ParameterlessCommand(Action execute, Func<bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? new Func<bool>(() => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute() => canExecute();
        public void Execute() => execute();
        public bool TryExecute()
        {
            if (canExecute())
            {
                execute();
                return true;
            }
            return false;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        bool ICommand.CanExecute(object? parameter) => canExecute();

        void ICommand.Execute(object? parameter) => execute();
    }

    class ParametricCommand<T> : ICommand
    {
        private readonly Action<T?> execute;
        private readonly Func<T?, bool> canExecute;

        public ParametricCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? new Func<T?, bool>(_ => true);
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(T? parameter) => canExecute(parameter);
        public void Execute(T? parameter) => execute(parameter);
        public bool TryExecute(T? parameter)
        {
            if (canExecute(parameter))
            {
                execute(parameter);
                return true;
            }
            return false;
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        bool ICommand.CanExecute(object? parameter) => canExecute(parameter is object o ? (T)o : default);

        void ICommand.Execute(object? parameter) => execute((T)parameter);
    }
}
