using Server.DataModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data_Access
{
    public class Database : SQLiteConnection
    {
        public const String DbName = "ProjectDB";
        public Database()
            : base(DbName)
        {
            CreateTable<User>();
        }
        public User QueryUser(string userLogin)
        {
            return (from u in Table<User>()
                    where u.Login == userLogin
                    select u).FirstOrDefault();
        }
        public User QueryUserLogin(string userLogin, string userPassword)
        {
            return (from u in Table<User>()
                    where u.Login == userLogin && u.Password == userPassword
                    select u).FirstOrDefault();
        }
        public IEnumerable<User> QueryAllUsers()
        {
            return from u in Table<User>()
                   orderby u.Login
                   select u;
        }

        public void RegisterUser(string userLogin, string userPassword)
        {
            var user = new User { Login = userLogin, Password = userPassword };
            Insert(user);
        }

        public bool LoginUser(string userLogin, string userPassword)
        {
            var user = QueryUserLogin(userLogin, userPassword);
            if (user == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
