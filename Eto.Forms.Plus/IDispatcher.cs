using System;
using System.Threading.Tasks;

namespace Eto.Forms.Plus
{
    public interface IDispatcher
    {
        /// <summary>
        /// Execute asynchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Post(Action action);

        /// <summary>
        /// Execute synchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Send(Action action);

        /// <summary>
        /// Gets a value indicating whether the current thread is the thread being dispatched to
        /// </summary>
        bool IsCurrent { get; }
    }

    public interface IDispatcherAsync : IDispatcher
    {
        /// <summary>
        /// Execute asynchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        new Task Post(Action action);
    }

    /// <summary>
    /// SyncronousDispatcher to run on the current thread the actions
    /// </summary>
    internal class SynchronousDispatcher : IDispatcher
    {
        public void Post(Action action)
        {
            action();
        }

        public void Send(Action action)
        {
            action();
        }

        public bool IsCurrent
        {
            get { return true; }
        }
    }

    /// <summary>
    /// This dispatcher uses Task from System.Threading, if you have Context Sensitive use the Send Method, it runs Syncronously
    /// To any change in this code and/or implementation refer to https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    /// </summary>
    internal class TaskDispatcher : IDispatcherAsync
    {
        private bool TaskRunningIn = false;
        public bool IsCurrent => TaskRunningIn;

        public async Task Post(Action action)
        {
            int ThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            int TaskID = System.Threading.Thread.CurrentThread.ManagedThreadId;
            await Task.Run(() =>
            {
                TaskID = System.Threading.Thread.CurrentThread.ManagedThreadId;
                action();
            }).ConfigureAwait(false);
            TaskRunningIn = ThreadID == TaskID;
        }

        public void Send(Action action)
        {
            action();
            TaskRunningIn = true;
        }

        void IDispatcher.Post(Action action)
        {
        }
    }
}