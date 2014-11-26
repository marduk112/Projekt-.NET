using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IFriendsCollection
    {
        void Add(string nick, PresenceStatus status);
        void Remove(string nick);
        ImmutableSortedDictionary<string, PresenceStatus> ToSortedSet();
    }
}
