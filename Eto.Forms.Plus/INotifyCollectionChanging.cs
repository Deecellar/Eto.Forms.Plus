using System.Collections.Specialized;

namespace Eto.Forms.Plus
{
    public interface INotifyCollectionChanging
    {
        /// <summary>
        /// Occurs when the collection will change
        /// </summary>
        event NotifyCollectionChangedEventHandler CollectionChanging;
    }
}