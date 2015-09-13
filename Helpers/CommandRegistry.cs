using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Helpers
{
    /// <summary>
    /// Serves as a centralized place for registering and executing commands, such as server commands.
    /// </summary>
    /// <typeparam name="T">An ICommand-derived type representing the type of command this instance will take.</typeparam>
    public class CommandRegistry<T>
        where T : ICommand
    {
        private Dictionary<string, T> _commands;

        /// <summary>
        /// Initialises a new instance of the EPQMessenger.Helpers.CommandRegistry class.
        /// </summary>
        public CommandRegistry()
        {
            _commands = new Dictionary<string, T>();
        }

        /// <summary>
        /// Adds a command to the register so that it can be later accessed and executed.
        /// </summary>
        /// <param name="command">The T command object for the added command.</param>
        public void Register(T command)
        {
            _commands.Add(command.Name, command);
        }

        /// <summary>
        /// Executes a command by delegation through the various classes.
        /// </summary>
        /// <param name="name">The name of the command to execute.</param>
        /// <param name="args">The arguments to pass to the command.</param>
        /// <returns>The string that the command method returns. Intended for printing to the server log.</returns>
        public string Execute(string name, string[] args)
        {
            if (_commands.ContainsKey(name))
            {
                return _commands[name].Execute(args);
            }
            else
            {
                return "No such command.";
            }
        }

        /// <summary>
        /// Finds the help text for the specified command.
        /// </summary>
        /// <param name="name">The name of the command whose help text to find.</param>
        /// <returns>A string, containing the help text, or null if the command was not found.</returns>
        public string GetCommandHelp(string name)
        {
            if (_commands.ContainsKey(name))
            {
                return _commands[name].HelpText;
            }
            else return null;
        }

        /// <summary>
        /// Returns a list of all command names.
        /// </summary>
        /// <returns>A List of strings containing command names.</returns>
        public List<string> ListCommandNames()
        {
            return _commands.Keys.ToList<string>();
        }
    }
}
