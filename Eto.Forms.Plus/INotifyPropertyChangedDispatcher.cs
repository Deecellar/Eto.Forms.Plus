using System;

namespace Eto.Forms.Plus
{
    public interface INotifyPropertyChangedDispatcher
    {
        /// <summary>
        /// Gets or sets the dispatcher to use.
        /// Called with an action, which should itself be called in the appropriate context
        /// </summary>
        Action<Action> PropertyChangedDispatcher { get; set; }
    }
}