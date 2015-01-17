using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server.Modules
{
    class NotificationFactory
    {
        public static List<PresenceStatusNotification> CreatePresenceNotifications(
            List<Common.User> usersList, DataModels.User whoChanged)
        {
            var notificationsList = new List<PresenceStatusNotification>();
            foreach (var user in usersList)
            {
                var notification = new PresenceStatusNotification
                {
                    Login = whoChanged.Login,
                    PresenceStatus = whoChanged.Status,
                    Recipient = user.Login
                };
                notificationsList.Add(notification);
            }
            return notificationsList;
        } 
    }
}
