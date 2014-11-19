using System;
using System.IO;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServiceStack.Text;

namespace Client.Modules
{
    public class Login
    {
        public Login()
        {
            var factory = new ConnectionFactory { HostName = Const.HostName};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare();
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(replyQueueName, true, consumer);
        }
        public AuthResponse LoginAuthRequestResponse(string login, string password)
        {
            var authRequest = new AuthRequest();
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            authRequest.Login = login;
            authRequest.Password = password;

            var messageBytes = authRequest.Serialize();//message forward login and password
            channel.BasicPublish("login", "regLogServer", props, messageBytes);

            while (true)
            {
                var ea = consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    return ea.Body.DeserializeAuthResponse();
                }
            }
        }
        public void Close()
        {
            connection.Close();
        }
        
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
    }
}
