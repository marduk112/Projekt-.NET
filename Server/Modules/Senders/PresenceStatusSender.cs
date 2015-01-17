using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;

namespace Server.Modules
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
                channel.ExchangeDeclare(Const.ClientExchange, "topic");
                channel.QueueDeclare(SenderName, false, false, false, null);
                channel.BasicQos(0, 1, false);
                channel.QueueBind(SenderName, Const.ClientExchange, routingKey);
                var responseBytes = notifcation.Serialize();
                var replyProps = channel.CreateBasicProperties();
                channel.BasicPublish(Const.ClientExchange, routingKey, replyProps, responseBytes);
            }
        }
    }
}
