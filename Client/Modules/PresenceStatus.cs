using System;
using System.Collections.Generic;
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
        //to determine
        public void SendPresenceStatus(User user, List<string> recipients)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("PresenceStatus", "topic");
                    var presenceStatus = new PresenceStatusNotification();
                    presenceStatus.PresenceStatus = user.Status;
                    presenceStatus.Login = user.Login;
                    var body = presenceStatus.Serialize();
                    foreach (var recipient in recipients)
                    {
                        channel.BasicPublish("PresenceStatus", "PresenceStatus." + recipient, null, body);
                    }
                }
            }
        }

        public PresenceStatusNotification PresenceStatusResponse (string login)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("PresenceStatus", "topic");
                    var queueName = channel.QueueDeclare();
                    channel.QueueBind(queueName, "PresenceStatus", "PresenceStatus." + login);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = body.DeserializePresenceStatusNotification();
                    return message;
                }
            }
        }
    }
}
