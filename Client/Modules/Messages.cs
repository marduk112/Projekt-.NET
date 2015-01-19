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
            channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
            queueName = channel.QueueDeclare();
            var routingKey = Const.ClientMessageNotificationRoute + Const.User.Login;
            channel.QueueBind(queueName, Const.ClientExchange, routingKey);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
        }
        public void SendMessage (MessageReq message)
        {
            var body = message.Serialize();
            var properties = channel.CreateBasicProperties();
            properties.SetPersistent(true);
            var routingKey = Const.ServerMessageRequestRoute;
            channel.BasicPublish(Const.ClientExchange, routingKey, properties, body);
        }
        //every user has queue for message response
        public MessageResponse ReceiveMessage()
        {
            var ea = consumer.Queue.Dequeue();
                //return null;
            var body = ea.Body;
            var message = body.DeserializeMessageResponse();
            var response = new MessageResponse
            {
                Attachment = message.Attachment,
                Message = message.Message,
                Login = message.Login,
                Recipient = message.Recipient,
                SendTime = message.SendTime,
                Status = Status.OK
            };
            channel.BasicAck(ea.DeliveryTag, false);
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
