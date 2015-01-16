using Common;
using Server.DataModels;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User = Server.DataModels.User;

namespace Server.Data_Access
{
    public class Database : SQLiteConnection
    {
        private const String DbName = "ProjectDB";

        public Database()
            : base(DbName)
        {
            //DropTable<User>();
            //DropTable<Friends>();

            CreateTable<User>();
            CreateTable<Friends>();
        }
        // User
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
            var user = new User { Login = userLogin, Password = userPassword, Status = PresenceStatus.Offline };
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

        // Friends
        public List<Common.User> QuerryAllFriends(string userLogin)
        {
            var userList = new List<Common.User>();
            var users = from u in Table<Friends>()
                where u.UserLogin1 == userLogin || u.UserLogin2 == userLogin
                select u;
            foreach (var user in users)
            {
                var login = user.UserLogin1 == userLogin ? user.UserLogin2 : user.UserLogin1;
                var userFriend = new Common.User { Login = login, Status = QueryUser(login).Status };
                userList.Add(userFriend);
            }
            return userList;
        }

        public void AddFriend(string userLogin, string friendLogin)
        {
            var isFriend = (from f in Table<Friends>()
                where
                    (f.UserLogin1 == userLogin && f.UserLogin2 == friendLogin) ||
                    (f.UserLogin2 == userLogin && f.UserLogin1 == friendLogin)
                select f).FirstOrDefault();
            if (isFriend == null)
            {
                var newFriends = new Friends { UserLogin1 = userLogin, UserLogin2 = friendLogin };
                Insert(newFriends);
            }
        }
    }
}
