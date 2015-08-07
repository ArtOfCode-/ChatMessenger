using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPQMessenger.Workers
{
    /// <summary>
    /// A derivative of Logger, with a different file, made for logging conversation messages.
    /// </summary>
    class ConversationLogger : Logger
    {
        /// <summary>
        /// The name of the log file. Returns a name of the format year-month-day.conv.log.
        /// </summary>
        public new string LogFile
        {
            get
            {
                DateTime now = DateTime.Now;
                return string.Format("{0}-{1}-{2}.conv.log", now.Year, now.Month, now.Day);
            }
        }
    }
}
