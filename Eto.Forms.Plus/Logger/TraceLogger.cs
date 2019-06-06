using System;
using System.Diagnostics;

namespace Eto.Forms.Plus.Logger
{
    public class TraceLogger : ILogger
    {
        private readonly string name;

        public TraceLogger(string name)
        {
            this.name = name;
        }

        public void Error(Exception exception, string message = null)
        {
            if (message == null)
            {
                Trace.WriteLine(String.Format("ERROR: [{1}] {0}", exception, this.name), "Eto.Forms.Plus");
            }
            else
            {
                Trace.WriteLine(String.Format("ERROR: [{2}] {0} {1}", message, exception, this.name), "Eto.Forms.Plus");
            }
        }

        public void Info(string format, params object[] args)
        {
            Trace.WriteLine(String.Format("INFO: [{1}] {0}", String.Format(format, args), this.name), "Eto.Forms.Plus");
        }

        public void Warn(string format, params object[] args)
        {
            Trace.WriteLine(String.Format("WARN: [{1}] {0}", String.Format(format, args), this.name), "Eto.Forms.Plus");
        }
    }
}