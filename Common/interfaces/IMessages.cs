using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IMessages
    {
        void SendMessage(string sender, string recipient, MessageReq message);
        IMessageResponse ReceiveMessage(string login);
    }
}
