using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.interfaces
{
    public interface IUsersList
    {
        IUserListResponse UserListReqResponse(string login);
        void CloseConnection();
    }
}
