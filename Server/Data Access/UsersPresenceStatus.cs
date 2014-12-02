using System.Collections.Immutable;
using System.Collections.Specialized;
using Common;

namespace Server.Data_Access
{
    public sealed class UsersPresenceStatus : INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public void Add(string nick, PresenceStatus status)
        {
            if (CollectionChanged != null)
            {
                if (!_dictionary.ContainsKey(nick))
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
                else
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
            }
            _dictionary.Add(nick, status);
        }
        public void Remove(string nick)
        {
            _dictionary.Remove(nick);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
            }
        }
        public ImmutableSortedDictionary<string, PresenceStatus> ReturnDictionary()
        {
            return _dictionary;
        }

        public User GetUser(string nick)
        {
            var user = new User {Login = nick, Status = _dictionary[nick]};
            return user;
        }
        //synchronized sorted dictionary
        private readonly ImmutableSortedDictionary<string, PresenceStatus> _dictionary = ImmutableSortedDictionary.Create<string, PresenceStatus>();
    }
}