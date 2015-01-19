using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.Interfaces
{
    public interface IMessages : IDisposable
    {
        void SendMessage(MessageReq message);
        MessageResponse ReceiveMessage();
    }
}
