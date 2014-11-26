using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IPresenceStatus
    {
        void SendPresenceStatus(User user, List<string> recipients);
        IPresenceStatusNotification PresenceStatusResponse(string login);
    }
}
