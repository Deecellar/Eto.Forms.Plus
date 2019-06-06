using System;

namespace Eto.Forms.Plus.Logger
{
    public interface ILogger
    {
        /// <summary>
        /// Log a message as Information
        /// </summary>
        /// <param name="format">A string with format</param>
        /// <param name="args">Params of format</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Log a message to warn
        /// </summary>
        /// <param name="format">A string with format</param>
        /// <param name="args">Params of format</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// Log a exception as an error
        /// </summary>
        /// <param name="exception">Exceotion to Log</param>
        /// <param name="message">Additional info about the Error</param>
        void Error(Exception exception, string message = null);
    }
}