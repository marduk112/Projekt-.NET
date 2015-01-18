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
    /// <summary>
    /// Used to send MessageNotifications to user
    /// </summary>
    class MessageSender : INotificationSender
    {
        private static string SenderName = "MessageNotificationSender";
        public void Send(Common.Notification notification)
        {
            var notif = (Common.MessageNotification)notification;

            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var routingKey = Const.ClientMessageNotificationRoute + notif.Recipient;
                channel.ExchangeDeclare(Const.ClientExchange, "topic");
                var queueName = channel.QueueDeclare();
                channel.QueueBind(SenderName, Const.ClientExchange, routingKey);
                var responseBytes = notif.Serialize();
                var replyProps = channel.CreateBasicProperties();
                channel.BasicPublish(Const.ClientExchange, routingKey, replyProps, responseBytes);
            }
        }
    }
}
