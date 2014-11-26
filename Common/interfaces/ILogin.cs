using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface ILogin
    {
        IAuthResponse LoginAuthRequestResponse(string login, string password);
        void Close();
    }
}
