/*
 * Mark Diedericks
 * 17/06/2018
 * Version 1.0.0
 * Relay command for actions
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Macro_Editor.Routing
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> ExecuteAction;
        private readonly Predicate<object> CanExecuteAction;

        /// <summary>
        /// Instantiate a relay command
        /// </summary>
        /// <param name="action">The action to be called</param>
        public RelayCommand(Action<object> action) : this(action, _ => true)
        {

        }

        /// <summary>
        /// Instantiate a relay command
        /// </summary>
        /// <param name="action">The action to be called</param>
        /// <param name="canExecute">The predicate of the action to be called</param>
        public RelayCommand(Action<object> action, Predicate<object> canExecute)
        {
            ExecuteAction = action;
            CanExecuteAction = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        /// <summary>
        /// Checks if a command can be executed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns>Bool if can be executed</returns>
        public bool CanExecute(object parameter)
        {
            return CanExecuteAction(parameter);
        }

        /// <summary>
        /// Executes a command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            ExecuteAction(parameter);
        }
    }
}
