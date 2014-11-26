using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IRegistration
    {
        ICreateUserResponse registration(string login, string password);
        void CloseConnection();
    }
}
