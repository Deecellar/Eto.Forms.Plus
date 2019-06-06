using System;
using System.Windows.Input;

namespace Eto.Forms.Plus
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _executeDelegate;
        private readonly Predicate<object> _canExecutePredicate;

        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> executeDelegate,
            Predicate<object> canExecutePredicate = null)
        {
            _executeDelegate = executeDelegate;
            _canExecutePredicate = canExecutePredicate;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecutePredicate == null ?
                true :
                _canExecutePredicate(parameter);
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }
    }
}