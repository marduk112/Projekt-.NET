using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.interfaces
{
    public interface IActivity
    {
        void ActivityReq(string login, bool isWriting, string recipient);
        IActivityResponse ActivityResponse(string login);
    }
}
