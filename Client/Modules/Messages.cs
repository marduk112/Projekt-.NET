using System;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class Messages : IMessages
    {
        public Messages(IConnectionFactory factory)
        {
            //var factory = new ConnectionFactory() { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare("messages", "topic", true);
            queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, "messages", Const.User.Login);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
        }
        public void SendMessage (MessageReq message)
        {
            var body = message.Serialize();
            var properties = channel.CreateBasicProperties();
            properties.SetPersistent(true);
            var routingKey = message.Recipient;
            channel.BasicPublish("messages", routingKey, properties, body);
            //(to determine)Server receive message and send it to correct user and save in database
            //channel.BasicPublish("messages", "Server", properties, body);
        }
        //every user has queue for message response
        public MessageResponse ReceiveMessage(int timeout)
        {
            BasicDeliverEventArgs ea;
            if (!consumer.Queue.Dequeue(timeout, out ea))
                return null;
            var body = ea.Body;
            var message = body.DeserializeMessageReq();
            var response = new MessageResponse
            {
                Attachment = message.Attachment,
                Message = message.Message,
                Recipient = message.Login,
                SendTime = message.SendTime,
                Status = Status.OK
            };
            return response;
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
        ~Messages()
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
        private string queueName;
        private QueueingBasicConsumer consumer;
        private IConnection connection;
        // private IConnection connection;

        /*public MessageResponse ReceiveMessageHistory(string yourLogin, string interlocutor)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic", true);
                    var queueName = channel.QueueDeclare();
                    channel.QueueBind(queueName, "messages", yourLogin);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = body.DeserializeMessageResponse();
                    return message;
                }
            }
        }*/
    }
}
