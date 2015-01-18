using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using Server.Modules.Interfaces;

namespace Server.Modules.Senders
{
    class PresenceStatusSender : INotificationSender
    {
        private static string SenderName = "PresenceStatusNotificationSender";
        public void Send(Common.Notification notif)
        {
            var notifcation = (Common.PresenceStatusNotification) notif;

            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var routingKey = Const.ClientPresenceStatusNotificationRoute + notifcation.Login;
                channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
                var responseBytes = notifcation.Serialize();
                channel.BasicPublish(Const.ClientExchange, routingKey, null, responseBytes);
            }
        }
    }
}
