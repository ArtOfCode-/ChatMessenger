using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace EPQMessenger.Workers
{
    /// <summary>
    /// A logger. Should be self-explanatory.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// Gets the directory the log file is contained in. Is the containing folder of the
        /// currently executing assembly.
        /// </summary>
        public string LogDirectory
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        /// <summary>
        /// Gets the log file name. Returns a name of format year-month-day.log.
        /// </summary>
        public string LogFile
        {
            get
            {
                DateTime now = DateTime.Now;
                return string.Format("{0}-{1}-{2}.log", now.Year, now.Month, now.Day);
            }
        }

        /// <summary>
        /// Sets whether or not file logging is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Initialises a new instance of the Logger class.
        /// </summary>
        public Logger()
        {

        }

        /// <summary>
        /// Logs a message to the log file.
        /// </summary>
        /// <param name="message">The message to log, optionally with formatting tags.</param>
        /// <param name="args">The arguments to insert into any formatting tags.</param>
        public void Log(string message, params object[] args)
        {
            if (IsEnabled)
            {
                StackFrame callerFrame = new StackFrame(1);
                MethodBase callerMethod = callerFrame.GetMethod();
                string[] splitFullTypeName = callerMethod.DeclaringType.Name.Split('.');
                string simpleTypeName = splitFullTypeName[splitFullTypeName.Count() - 1];
                string callerName = simpleTypeName + "." + callerMethod.Name;
                string messageText = string.Format("[{0}] [{1}] {2}", DateTime.Now.ToLongTimeString(),
                    callerName, string.Format(message, args));
                File.AppendAllLines(LogDirectory + "\\" + LogFile, new string[] { messageText });
            }
        }

        /// <summary>
        /// Logs a message to the log file, without including method information about the calling method.
        /// </summary>
        /// <param name="message">The message to log, optionally with formatting tags.</param>
        /// <param name="args">The arguments to insert into any formatting tags.</param>
        public void LogWithoutCallerInfo(string message, params object[] args)
        {
            if (IsEnabled)
            {
                string messageText = string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(),
                   string.Format(message, args));
                File.AppendAllLines(LogDirectory + "\\" + LogFile, new string[] { messageText });
            }
        }
    }
}
