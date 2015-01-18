using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    //to determine
    public class PresenceStatus : IPresenceStatus
    {
        public PresenceStatus(IConnectionFactory factory)
        {
            //var factory = new ConnectionFactory { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(Const.ClientPresenceStatusNotificationRoute, "topic", true);
            queueName = channel.QueueDeclare();
            var routingKey = Const.ClientPresenceStatusNotificationRoute + Const.User.Login;
            channel.QueueBind(queueName, Const.ClientExchange, routingKey);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
        }

        public void SendPresenceStatus(User message)
        {
            var messageBytes = message.Serialize();
            var props = channel.CreateBasicProperties();
            props.SetPersistent(true);
            channel.BasicPublish(Const.ClientExchange, "ChangeUserStatusServer", props, messageBytes);
            //send presence status directly to users
            //channel.BasicPublish("UsersStatus", message.Login, null, messageBytes);
        }

        public PresenceStatusNotification ReceiveUserPresenceStatus(int timeout)
        {
            BasicDeliverEventArgs ea;
            if (!consumer.Queue.Dequeue(timeout, out ea))
                return null;
            channel.BasicAck(ea.DeliveryTag, false);
            var body = ea.Body;
            var message = body.DeserializePresenceStatusNotification();
            return message;
        }
        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Free other state (managed objects).
                queueName = null;
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            CloseConnection();
            channel.Dispose();
            connection.Dispose();
            channel = null;
            connection = null;
            consumer = null;
            _disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        ~PresenceStatus()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }

        private void CloseConnection()
        {
            channel.Close();
            connection.Close();
        }
        private bool _disposed = false;
        private IModel channel;
        private IConnection connection;
        private string queueName;
        private QueueingBasicConsumer consumer;
    }
}
