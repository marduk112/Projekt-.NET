using Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataModels
{
    public class User
    {
        [PrimaryKey, MaxLength(32)]
        public string Login { get; set; }
        [MaxLength(128), NotNull]
        public string Password { get; set; }
        [NotNull]
        public PresenceStatus Status { get; set; }
    }

    public class Database : SQLiteConnection
    {
        private const String DbName = "ProjectDB";

        public Database() : base(DbName)
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
            var user = (from u in Table<User>()
                    where u.Login == userLogin && u.Password == userPassword
                    select u).FirstOrDefault();
            ChangeUserStatus(ref user, PresenceStatus.Online);
            return user;
        }

        public void ChangeUserStatus(ref User user, PresenceStatus status)
        {
            user.Status = status;
            Update(user);
        }
        public IEnumerable<User> QueryAllUsers()
        {
            return from u in Table<User>()
                   orderby u.Login
                   select u;
        }

        public void RegisterUser(string userLogin, string userPassword)
        {
            var user = new User {Login = userLogin, Password = userPassword, Status = PresenceStatus.Offline };
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
