using System;
using System.Windows.Input;

namespace WpfAnalyzerGUI.Commands;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _action;
    private readonly Predicate<object?>? _predicate;

    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _action = execute ?? throw new ArgumentNullException(nameof(execute));
        _predicate = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        return _predicate == null || _predicate(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    void ICommand.Execute(object? parameter)
    {
        _action(parameter);
    }
}
