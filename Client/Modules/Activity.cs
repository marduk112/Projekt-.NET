using System;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class Activity : IActivity
    {
        public Activity(IConnectionFactory factory)
        {
            //var factory = new ConnectionFactory() { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
            queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, Const.ClientExchange, "Activity." + Const.User.Login);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
        }

        public void ActivityReq(ActivityReq activityReq)
        {
            var body = activityReq.Serialize();
            channel.ConfirmSelect();
            channel.BasicPublish(Const.ClientExchange, "Activity." + activityReq.Recipient, null, body);
            channel.WaitForConfirmsOrDie();
            channel.QueueDelete(queueName);
        }

        public ActivityResponse ActivityResponse(int timeout)
        {
            var ea = consumer.Queue.Dequeue();
                //return null;
            var body = ea.Body;
            var message = body.DeserializeActivityReq();
            var activityResponse = new ActivityResponse
            {
                Login = message.Login,
                Recipient = message.Recipient,
                Status = Status.OK,
                IsWriting = message.IsWriting
            };
            return activityResponse;
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
        ~Activity()
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
