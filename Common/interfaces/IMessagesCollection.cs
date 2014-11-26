using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IMessagesCollection
    {
        void Add(string nick, MessageNotification message);
        void Remove(string nick);
        SortedSet<MessageNotification> GetConversationWith(string nick);
        void AddConversationWith(string nick, ICollection<MessageNotification> message);
    }
}
