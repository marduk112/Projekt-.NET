using System;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class Activity : IDisposable
    {
        public Activity(string login)
        {
            this.login = login;
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare("activity", "topic");
            queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, "activity", "Activity." + login);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
        }

        public void ActivityReq(ActivityReq activityReq)
        {
            var body = activityReq.Serialize();
            channel.BasicPublish("activity", "Activity."+activityReq.Recipient, null, body);
        }

        public ActivityResponse ActivityResponse()
        {
            var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
            var body = ea.Body;
            var message = body.DeserializeActivityReq();
            var activityResponse = new ActivityResponse();
            activityResponse.Recipient = message.Login;
            activityResponse.Status = Status.OK;
            activityResponse.IsWriting = message.IsWriting;
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
        private string login;
        private IModel channel;
        private IConnection connection;
        private string queueName;
        private QueueingBasicConsumer consumer;
    }
}
