using StyletIoC;
using System;

namespace Eto.Forms.Plus
{
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
    public interface IWindowManagerConfig
    {
        /// <summary>
        /// Returns the currently-displayed window, or null if there is none (or it can't be determined)
        /// </summary>
        /// <returns>The currently-displayed window, or null</returns>
        Window GetActiveWindow();
    }

    /// <summary>
    /// Default implementation of IWindowManager, is capable of showing a ViewModel's View as a dialog or a window
    /// </summary>

    public class WindowManager
    {
        private readonly IContainer _container;
        private readonly ViewFactory _viewFactory;
        private readonly Application _application;

        public WindowManager(IContainer container, ViewFactory viewFactory,
            Application application)
        {
            _container = container;
            _viewFactory = viewFactory;
            _application = application;
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
            return _viewFactory.GetAndBind(viewModel) as Control;
        }
    }
}