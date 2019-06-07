using Eto.Forms.Plus.Logger;
using System;
using System.ComponentModel;

namespace Eto.Forms.Plus
{
    public interface IWindowManagerConfig
    {
        /// <summary>
        /// Returns the currently-displayed window, or null if there is none (or it can't be determined)
        /// </summary>
        /// <returns>The currently-displayed window, or null</returns>
        Window GetActiveWindow();
    }
    /// <summary>
    /// Manager capable of taking a ViewModel instance, instantiating its View and showing it as a dialog or window
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        void ShowWindow(object viewModel);

        /// <summary>
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        bool? ShowDialog(object viewModel);

        /// <summary>
        /// Display a MessageBox
        /// </summary>
        /// <param name="messageBoxText">A <see cref="System.String"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="System.String"/> that specifies the title bar caption to display.</param>
        /// <param name="buttons">A <see cref="System.Windows.MessageBoxButton"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="System.Windows.MessageBoxImage"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the default result of the message box.</param>
        /// <param name="cancelResult">A <see cref="System.Windows.MessageBoxResult"/> value that specifies the cancel result of the message box</param>
        /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
        /// <param name="flowDirection">The <see cref="System.Windows.FlowDirection"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultFlowDirection"/></param>
        /// <param name="textAlignment">The <see cref="System.Windows.TextAlignment"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultTextAlignment"/></param>
        /// <returns>The result chosen by the user</returns>
        //DialogResult ShowMessageBox(string messageBoxText, string caption = "",
        //    MessageBoxButtons buttons = MessageBoxButtons.OK,
        //    MessageBoxType icon = MessageBoxType.Information,
        //    MessageBoxResult defaultResult = MessageBoxResul t.None,
        //    MessageBoxResult cancelResult = MessageBoxResult.None,
        //    IDictionary<MessageBoxResult, string> buttonLabels = null,
        //    FlowDirection? flowDirection = null,
        //    TextAlignment? textAlignment = null);
    }

    /// <summary>
    /// Configuration passed to WindowManager (normally implemented by BootstrapperBase)
    /// </summary>


    /// <summary>
    /// Default implementation of IWindowManager, is capable of showing a ViewModel's View as a dialog or a window
    /// </summary>

    public class WindowManager
    {
        private readonly ILogger logger;
        private readonly StyletIoC.IContainer _container;
        private readonly ViewFactory _viewFactory;
        private readonly Application _application;

        public WindowManager(StyletIoC.IContainer container, ViewFactory viewFactory,
            Application application)
        {
            _container = container;
            _viewFactory = viewFactory;
            _application = application;
            this.logger = LogManager.GetLogger(typeof(NullLogger));
        }

        public void Exit()
        {
            _application.Quit();
        }

        public void RunOnUIThread(Action action)
        {
            _application.Invoke(action);
        }

        public T RunOnUIThread<T>(Func<T> func)
        {
            return _application.Invoke(func);
        }

        public TResult ShowDialog<TViewModel, TResult>(Control owner = null)
        {
            var control = CreateAndBind<TViewModel>() as Dialog<TResult>;
            if (control == null)
                return default(TResult);
            if (owner != null)
                return control.ShowModal(owner);
            else
                return control.ShowModal();
        }

        public void ShowForm<TViewModel>()
        {
            var control = CreateAndBind<TViewModel>() as Form;
            control?.Show();
        }

        public Control CreateAndBind<TViewModel>()
        {
            var viewModel = _container.Get<TViewModel>();
            var c = _viewFactory.GetAndBind(viewModel);
            new FormConductor(c, viewModel);
            return c as Control;
        }

        private class FormConductor : IChildDelegate
        {
            private readonly Window window;
            private readonly object viewModel;
            private ILogger logger;

            public FormConductor(Window window, object viewModel)
            {
                this.window = window;
                this.viewModel = viewModel;
                logger = LogManager.GetLogger(typeof(NullLogger));
                // They won't be able to request a close unless they implement IChild anyway...
                var viewModelAsChild = this.viewModel as IChild;
                if (viewModelAsChild != null)
                    viewModelAsChild.Parent = this;

                ScreenExtensions.TryActivate(this.viewModel);

                var viewModelAsScreenState = this.viewModel as IScreenState;
                if (viewModelAsScreenState != null)
                {
                    window.WindowStateChanged += this.WindowStateChanged;
                    window.Closed += this.WindowClosed;
                }

                if (this.viewModel is IGuardClose)
                    window.Closing += this.WindowClosing;
            }

            private void WindowStateChanged(object sender, EventArgs e)
            {
                switch (this.window.WindowState)
                {
                    case WindowState.Maximized:
                    case WindowState.Normal:
                        logger.Info("Window {0} maximized/restored: activating", this.window);
                        ScreenExtensions.TryActivate(this.viewModel);
                        break;

                    case WindowState.Minimized:
                        logger.Info("Window {0} minimized: deactivating", this.window);
                        ScreenExtensions.TryDeactivate(this.viewModel);
                        break;
                }
            }

            private void WindowClosed(object sender, EventArgs e)
            {
                // Logging was done in the Closing handler

                this.window.WindowStateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing; // Not sure this is required

                ScreenExtensions.TryClose(this.viewModel);
            }

            private async void WindowClosing(object sender, CancelEventArgs e)
            {
                if (e.Cancel)
                    return;

                logger.Info("ViewModel {0} close requested because its View was closed", this.viewModel);

                // See if the task completed synchronously
                var task = ((IGuardClose)this.viewModel).CanCloseAsync();
                if (task.IsCompleted)
                {
                    // The closed event handler will take things from here if we don't cancel
                    if (!task.Result)
                        logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    e.Cancel = !task.Result;
                }
                else
                {
                    e.Cancel = true;
                    logger.Info("Delaying closing of ViewModel {0} because CanCloseAsync is completing asynchronously", this.viewModel);
                    if (await task)
                    {
                        this.window.Closing -= this.WindowClosing;
                        this.window.Close();
                        // The Closed event handler handles unregistering the events, and closing the ViewModel
                    }
                    else
                    {
                        logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    }
                }
            }

            /// <summary>
            /// Close was requested by the child
            /// </summary>
            /// <param name="item">Item to close</param>
            /// <param name="dialogResult">DialogResult to close with, if it's a dialog</param>
            async void IChildDelegate.CloseItem(object item, bool? dialogResult)
            {
                if (item != this.viewModel)
                {
                    logger.Warn("IChildDelegate.CloseItem called with item {0} which is _not_ our ViewModel {1}", item, this.viewModel);
                    return;
                }

                var guardClose = this.viewModel as IGuardClose;
                if (guardClose != null && !await guardClose.CanCloseAsync())
                {
                    logger.Info("Close of ViewModel {0} cancelled because CanCloseAsync returned false", this.viewModel);
                    return;
                }

                logger.Info("ViewModel {0} close requested with DialogResult {1} because it called RequestClose", this.viewModel, dialogResult);

                this.window.WindowStateChanged -= this.WindowStateChanged;
                this.window.Closed -= this.WindowClosed;
                this.window.Closing -= this.WindowClosing;

                // Need to call this after unregistering the event handlers, as it causes the window
                // to be closed
                //if (dialogResult != null)
                //    this.window.Result = dialogResult;

                ScreenExtensions.TryClose(this.viewModel);

                window.Close();

            }
        }
    }
}