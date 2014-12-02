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
    //to determine
    public class PresenceStatus : IDisposable
    {
        public PresenceStatus()
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare("UsersStatus", "topic");
            queueName = channel.QueueDeclare();
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
        }

        public void SendPresenceStatus(User message)
        {
            var messageBytes = message.Serialize();
            channel.BasicPublish("UsersStatus", "Server", null, messageBytes);
            //send presence status directly to users
            //channel.BasicPublish("UsersStatus", message.Login, null, messageBytes);
        }

        public User ReceiveUserPresenceStatus()
        {
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
            channel.QueueBind(queueName, "UsersStatus", nick);
            channel.BasicConsume(queueName, true, consumer);
        }

        public void RemoveFriendToListenPresenceStatus(string nick)
        {
            channel.QueueUnbind(queueName, "UsersStatus", nick, null);
            channel.BasicConsume(queueName, true, consumer);
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
