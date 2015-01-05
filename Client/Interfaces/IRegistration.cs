using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Client.Interfaces
{
    public interface IRegistration
    {
        CreateUserResponse registration(CreateUserReq createUserReq);
    }
}
