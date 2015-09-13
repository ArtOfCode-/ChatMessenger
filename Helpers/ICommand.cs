using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Helpers
{
    public interface ICommand
    {
        bool IsPrivileged { get; set; }

        string Name { get; set; }

        string HelpText { get; set; }

        string Execute(string[] args);
    }
}
