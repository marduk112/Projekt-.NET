using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using Common;

namespace Client.Notifies
{
    //to determube
    public sealed class MessagesCollection : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void Add(string nick, MessageNotification message)
        {
            if (!_dictionary.ContainsKey(nick))
                _dictionary.Add(nick, new SortedSet<MessageNotification>(new Comparator()));
            _dictionary[nick].Add(message);
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
        }
        public void Remove(string nick)
        {
            _dictionary.Remove(nick);
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }
        public SortedSet<MessageNotification> GetConversationWith(string nick)
        {
            return _dictionary[nick];
        }
        public void AddConversationWith(string nick, ICollection<MessageNotification> message)
        {
            _dictionary[nick].UnionWith(message);
        }

        private readonly ImmutableDictionary<string, SortedSet<MessageNotification>> _dictionary = ImmutableDictionary.Create<string, SortedSet<MessageNotification>>();
    }

    internal class Comparator : IComparer<MessageNotification>
    {
        public int Compare(MessageNotification x, MessageNotification y)
        {
            return x.SendTime.CompareTo(y.SendTime);
        }
    }
}
