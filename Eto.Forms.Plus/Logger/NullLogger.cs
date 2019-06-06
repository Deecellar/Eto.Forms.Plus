using System;

namespace Eto.Forms.Plus.Logger
{
    public class NullLogger : ILogger
    {
        public void Error(Exception exception, string message = null)
        {
        }

        public void Info(string format, params object[] args)
        {
        }

        public void Warn(string format, params object[] args)
        {
        }
    }
}