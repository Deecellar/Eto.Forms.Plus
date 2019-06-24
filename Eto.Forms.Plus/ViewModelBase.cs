using Eto.Forms;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Eto.Forms.Plus
{
	public class ViewModelBase : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		public Control View { get; set; }

		protected void SetAndNotify<T>(ref T field, T value, [CallerMemberName] string memberName = null)
		{
			field = value;
			NotifyChanged(memberName);
		}

		protected void NotifyChanged([CallerMemberName] string memberName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
		}

		public virtual void OnViewLoaded()
		{
		}

		public virtual void OnViewShown()
		{
		}

        public virtual void OnViewUnloaded()
        {
        }

        public virtual void OnViewClosed()
        {
        }
    }
}
