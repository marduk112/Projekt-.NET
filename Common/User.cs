using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class User
    {
        public User(string login, string password)
        {
            this.Password = password;
            this.Login = login;
        }

        public String Login { get; private set;}
        public String Password { get; private set; }

        public enum State
        {
            Present,
            BeRightBack,
            Absent,
        }
    }
}
