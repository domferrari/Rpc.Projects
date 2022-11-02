using System;
using System.Windows.Input;

namespace Rpc.Bulletin.Maker;

public class Command : ICommand
{
	private readonly Action<object> _execute = null;
	private readonly Predicate<object> _canExecute = null;

	/// -----------------------------------------------------------------------------------------------------------
	public Command(Action<object> execute) : this(execute, null)
	{
	}

	/// -----------------------------------------------------------------------------------------------------------
	public Command(Action<object> execute, Predicate<object> canExecute)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	//public event EventHandler CanExecuteChanged
	//{
	//	add { CommandManager.RequerySuggested += value; }
	//	remove { CommandManager.RequerySuggested -= value; }
	//}
	public event EventHandler CanExecuteChanged;
	public bool CanExecute(object parameter) => _canExecute != null ? _canExecute(parameter) : true;
	public void Execute(object parameter) => _execute?.Invoke(parameter);
	public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
