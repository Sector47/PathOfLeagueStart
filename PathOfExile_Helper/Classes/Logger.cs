using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PathOfExile_Helper.Classes
{
    /// <summary>
    /// A class to handle logging information.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Method to log messages.
        /// </summary>
        /// <param name="messageString"> The message to be logged. </param>
        public static void Log(String messageString)
        {
            // Log messages
            Console.WriteLine(messageString);
        }

        /// <summary>
        /// Method to log an error.
        /// </summary>
        /// <param name="errorString"> The error string to be logged. </param>
        public static void LogError(String errorString)
        {
            // Log Errors
            Console.WriteLine(errorString);
        }

        /// <summary>
        /// Overloaded method for logging an error with an attached exception.
        /// </summary>
        /// <param name="errorString"> The error string to be logged. </param>
        /// <param name="exception"> The exception that was thrown. </param>
        public static void LogError(String errorString, Exception exception)
        {
            // Log Errors
            Console.WriteLine(errorString);
            Console.WriteLine(exception);
        }

        /// <summary>
        /// Method for logging debug information.
        /// </summary>
        /// <param name="debugMessageString"> The debug message to be logged. </param>
        public static void LogDebug(String debugMessageString)
        {
            // Log debug information
            Console.WriteLine(debugMessageString);
        }
    }
}
