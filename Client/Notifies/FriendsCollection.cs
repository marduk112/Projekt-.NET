using System.Collections.Immutable;
using System.Collections.Specialized;
using Common;

namespace Client.Notifies
{
    public sealed class FriendsCollection : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void Add(string nick, PresenceStatus status)
        {
            _dictionary.Add(nick, status);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            }
        }
        public void Remove(string nick)
        {
            _dictionary.Remove(nick);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            }
        }
        public ImmutableSortedDictionary<string, PresenceStatus> ToSortedSet()
        {
            return _dictionary;
        }
        //synchronized sorted dictionary
        private readonly ImmutableSortedDictionary<string, PresenceStatus> _dictionary = ImmutableSortedDictionary.Create<string, PresenceStatus>();
    }
}
