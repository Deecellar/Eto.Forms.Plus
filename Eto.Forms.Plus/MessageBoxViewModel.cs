using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;

namespace Eto.Forms.Plus
{
    /// <summary>
    /// Interface for a MessageBoxViewModel. MessageBoxWindowManagerExtensions.ShowMessageBox will use the configured implementation of this
    /// </summary>
    public interface IMessageBoxViewModel
    {
        /// <summary>
        /// Setup the MessageBoxViewModel with the information it needs
        /// </summary>
        /// <param name="messageBoxText">A <see cref="System.String"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="System.String"/> that specifies the title bar caption to display.</param>
        /// <param name="buttons">A <see cref="System.Windows.MessageBoxButtons"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="System.Windows.MessageBoxType"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="System.Windows.DialogResult"/> value that specifies the default result of the message box.</param>
        /// <param name="cancelResult">A <see cref="System.Windows.DialogResult"/> value that specifies the cancel result of the message box</param>
        /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
        /// <param name="flowDirection">The <see cref="System.Windows.FlowDirection"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultFlowDirection"/></param>
        /// <param name="textAlignment">The <see cref="System.Windows.TextAlignment"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultTextAlignment"/></param>
        void Setup(
            string messageBoxText,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxType icon = MessageBoxType.Information,
            DialogResult defaultResult = DialogResult.None,
            DialogResult cancelResult = DialogResult.None,
            IDictionary<DialogResult, string> buttonLabels = null,
            TextAlignment? textAlignment = null);

        /// <summary>
        /// Gets the button clicked by the user, after they've clicked it
        /// </summary>
        DialogResult ClickedButton { get; }
    }

    /// <summary>
    /// Default implementation of IMessageBoxViewModel, and is therefore the ViewModel shown by default by ShowMessageBox
    /// </summary>
    public class MessageBoxViewModel : Screen, IMessageBoxViewModel
    {
        /// <summary>
        /// Gets or sets the mapping of button to text to display on that button. You can modify this to localize your application.
        /// </summary>
        public static IDictionary<DialogResult, string> ButtonLabels { get; set; }

        /// <summary>
        /// Gets or sets the mapping of MessageBoxButtons values to the buttons which should be displayed
        /// </summary>
        public static IDictionary<MessageBoxButtons, DialogResult[]> ButtonToResults { get; set; }

        /// <summary>
        /// Gets or sets the mapping of MessageBoxType to the SystemIcon to display. You can customize this if you really want.
        /// </summary>
        public static IDictionary<MessageBoxType, Icon> IconMapping { get; set; }

        /// <summary>
        /// Gets or sets the mapping of MessageBoxType to the sound to play when the MessageBox is shown. You can customize this if you really want.
        /// </summary>
        public static IDictionary<MessageBoxType, SystemSound> SoundMapping { get; set; }

        /// <summary>
        /// Gets or sets the default <see cref="System.Windows.FlowDirection"/> to use
        /// </summary>

        /// <summary>
        /// Gets or sets the default <see cref="System.Windows.TextAlignment"/> to use
        /// </summary>
        public static TextAlignment DefaultTextAlignment { get; set; }

        static MessageBoxViewModel()
        {
            ButtonLabels = new Dictionary<DialogResult, string>()
            {
                { DialogResult.Ok, "OK" },
                { DialogResult.Cancel, "Cancel" },
                { DialogResult.Yes, "Yes" },
                { DialogResult.No, "No" },
            };

            ButtonToResults = new Dictionary<MessageBoxButtons, DialogResult[]>()
            {
                { MessageBoxButtons.OK, new[] { DialogResult.Ok} },
                { MessageBoxButtons.OKCancel, new[] { DialogResult.Ok, DialogResult.Cancel } },
                { MessageBoxButtons.YesNo, new[] { DialogResult.Yes, DialogResult.No } },
                { MessageBoxButtons.YesNoCancel, new[] { DialogResult.Yes, DialogResult.No, DialogResult.Cancel } },
            };

            SoundMapping = new Dictionary<MessageBoxType, SystemSound>()
            {
                { MessageBoxType.Error, SystemSounds.Hand },
                { MessageBoxType.Question, SystemSounds.Question },
                { MessageBoxType.Warning, SystemSounds.Exclamation },
                { MessageBoxType.Information, SystemSounds.Asterisk },
            };

            DefaultTextAlignment = TextAlignment.Left;
        }

        /// <summary>
        /// Setup the MessageBoxViewModel with the information it needs
        /// </summary>
        /// <param name="messageBoxText">A <see cref="System.String"/> that specifies the text to display.</param>
        /// <param name="caption">A <see cref="System.String"/> that specifies the title bar caption to display.</param>
        /// <param name="buttons">A <see cref="System.Windows.MessageBoxButtons"/> value that specifies which button or buttons to display.</param>
        /// <param name="icon">A <see cref="System.Windows.MessageBoxType"/> value that specifies the icon to display.</param>
        /// <param name="defaultResult">A <see cref="System.Windows.DialogResult"/> value that specifies the default result of the message box.</param>
        /// <param name="cancelResult">A <see cref="System.Windows.DialogResult"/> value that specifies the cancel result of the message box</param>
        /// <param name="buttonLabels">A dictionary specifying the button labels, if desirable</param>
        /// <param name="flowDirection">The <see cref="System.Windows.FlowDirection"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultFlowDirection"/></param>
        /// <param name="textAlignment">The <see cref="System.Windows.TextAlignment"/> to use, overrides the <see cref="MessageBoxViewModel.DefaultTextAlignment"/></param>
        public void Setup(
            string messageBoxText,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxType icon = MessageBoxType.Information,
            DialogResult defaultResult = DialogResult.None,
            DialogResult cancelResult = DialogResult.None,
            IDictionary<DialogResult, string> buttonLabels = null,
            TextAlignment? textAlignment = null)
        {
            this.Text = messageBoxText;
            this.DisplayName = caption;
            this.Icon = icon;

            var buttonList = new BindableCollection<LabelledValue<DialogResult>>();
            this.ButtonList = buttonList;
            foreach (var val in ButtonToResults[buttons])
            {
                string label;
                if (buttonLabels == null || !buttonLabels.TryGetValue(val, out label))
                    label = ButtonLabels[val];

                var lbv = new LabelledValue<DialogResult>(label, val);
                buttonList.Add(lbv);
                if (val == defaultResult)
                    this.DefaultButton = lbv;
                else if (val == cancelResult)
                    this.CancelButton = lbv;
            }
            // If they didn't specify a button which we showed, then pick a default, if we can
            if (this.DefaultButton == null)
            {
                if (defaultResult == DialogResult.None && this.ButtonList.Any())
                    this.DefaultButton = buttonList[0];
                else
                    throw new ArgumentException("DefaultButton set to a button which doesn't appear in Buttons");
            }
            if (this.CancelButton == null)
            {
                if (cancelResult == DialogResult.None && this.ButtonList.Any())
                    this.CancelButton = buttonList.Last();
                else
                    throw new ArgumentException("CancelButton set to a button which doesn't appear in Buttons");
            }

            this.TextAlignment = textAlignment ?? DefaultTextAlignment;
        }

        /// <summary>
        /// Gets or sets the list of buttons which are shown in the View.
        /// </summary>
        public IObservableCollection<LabelledValue<DialogResult>> ButtonList { get; protected set; }

        /// <summary>
        /// Gets or sets the item in ButtonList which is the Default button
        /// </summary>
        public LabelledValue<DialogResult> DefaultButton { get; protected set; }

        /// <summary>
        /// Gets or sets the item in ButtonList which is the Cancel button
        /// </summary>
        public LabelledValue<DialogResult> CancelButton { get; protected set; }

        /// <summary>
        /// Gets or sets the text which is shown in the body of the MessageBox
        /// </summary>
        public virtual string Text { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the Text contains many lines
        /// </summary>
        public virtual bool TextIsMultiline
        {
            get { return this.Text.Contains("\n"); }
        }

        /// <summary>
        /// Gets or sets the icon which the user specified
        /// </summary>
        public virtual MessageBoxType Icon { get; protected set; }

        /// <summary>
        /// Gets or the icon which is shown next to the text in the View
        /// </summary>
        public virtual Icon ImageIcon
        {
            get { return IconMapping[this.Icon]; }
        }

        /// <summary>
        /// Gets or sets which way the document should flow
        /// </summary>

        /// <summary>
        /// Gets or sets the text alignment of the message
        /// </summary>
        public virtual TextAlignment TextAlignment { get; protected set; }

        /// <summary>
        /// Gets or sets which button the user clicked, once they've clicked a button
        /// </summary>
        public virtual DialogResult ClickedButton { get; protected set; }

        /// <summary>
        /// When the View loads, play a sound if appropriate
        /// </summary>
        protected override void OnViewLoaded()
        {
            // There might not be a sound, or it might be null
            SystemSound sound;
            SoundMapping.TryGetValue(this.Icon, out sound);
            if (sound != null)
                sound.Play();
        }

        /// <summary>
        /// Called when MessageBoxView when the user clicks a button
        /// </summary>
        /// <param name="button">Button which was clicked</param>
        public void ButtonClicked(DialogResult button)
        {
            this.ClickedButton = button;
            this.RequestClose(true);
        }
    }
}