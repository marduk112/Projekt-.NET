using Common;
using Server.DataModels;
using Server.Modules;
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
        public Common.User QueryUserLogin(string userLogin, string userPassword)
        {
            var user = (from u in Table<User>()
                        where u.Login == userLogin && u.Password == userPassword
                        select u).FirstOrDefault();
            ChangeUserStatus(ref user, PresenceStatus.Online);
            return user.ToCommonUser();
        }

        public void ChangeUserStatus(ref User user, PresenceStatus status)
        {
            user.Status = status;
            Update(user);
            var friendsList = QueryAllFriends(user.Login);
            var notificationsList = NotificationFactory.CreatePresenceNotifications(friendsList, user);
            var sender = new PresenceStatusSender();
            foreach (var presenceStatusNotification in notificationsList)
            {
                sender.Send(presenceStatusNotification);
            }
        }
        public IEnumerable<Common.User> QueryAllUsers()
        {
            List<Common.User> userList = new List<Common.User>();
            var users = from u in Table<User>()
                   orderby u.Login
                   select u;
            foreach (var user in users)
            {
                userList.Add(user.ToCommonUser());
            }
            return userList;
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
        public List<Common.User> QueryAllFriends(string userLogin)
        {
            var userList = new List<Common.User>();
            var friends = from u in Table<Friends>()
                where u.UserLogin1 == userLogin || u.UserLogin2 == userLogin
                select u;
            foreach (var friend in friends)
            {
                // Defining friend login which isn't userLogin
                var friendLogin = friend.UserLogin1 == userLogin ? friend.UserLogin2 : friend.UserLogin1;
                var userFriend = new Common.User { Login = friendLogin, Status = QueryUser(friendLogin).Status };
                userList.Add(userFriend);
            }
            return userList;
        }

        private Friends AreFriends(string userLogin, string friendLogin)
        {
            return (from f in Table<Friends>()
                where
                    (f.UserLogin1 == userLogin && f.UserLogin2 == friendLogin) ||
                    (f.UserLogin2 == userLogin && f.UserLogin1 == friendLogin)
                select f).FirstOrDefault();
        }

        public void AddFriend(string userLogin, string friendLogin)
        {
            if (AreFriends(userLogin, friendLogin) == null)
            {
                var newFriends = new Friends { UserLogin1 = userLogin, UserLogin2 = friendLogin };
                Insert(newFriends);
            }
        }

        public void DeleteFriend(string userLogin, string friendLogin)
        {
            var areFriends = AreFriends(userLogin, friendLogin);
            if (areFriends != null)
            {
                Delete(areFriends);
            }
        }
    }
}
