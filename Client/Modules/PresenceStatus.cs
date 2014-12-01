using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class PresenceStatus
    {
        public PresenceStatus()
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            var connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare("UsersStatus", "topic");
            queueName = channel.QueueDeclare();
        }

        public void SendPresenceStatus(User message)
        {
            var messageBytes = message.Serialize();
            channel.ExchangeDeclare("UsersStatus", "topic");
            channel.BasicPublish("UsersStatus", "Server", null, messageBytes);
        }

        public User ReceiveUserPresenceStatus()
        {
            channel.ExchangeDeclare("messages", "topic", true);
            var queueName = channel.QueueDeclare();
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            while (true)
            {
                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                var body = ea.Body;
                var message = body.DeserializeUser();
                return message;
            }
        }

        public void AddFriendToListenPresenceStatus(string nick)
        {
            channel.QueueBind(queueName, "messages", nick);
        }

        public void RemoveFriendToListenPresenceStatus(string nick)
        {
            channel.QueueUnbind(queueName, "messages", nick, null);
        }

        private IModel channel;
        private string queueName;
    }
}
