using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;

namespace Client.Modules
{
    public class Login : ILogin
    {
        public Login()
        {
            //_factory = factory;
            _factory = new ConnectionFactory {HostName = Const.HostName};
        }
        public AuthResponse LoginAuthRequestResponse(AuthRequest authRequest)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    replyQueueName = channel.QueueDeclare();
                    consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(replyQueueName, true, consumer);
                    var corrId = Guid.NewGuid().ToString();
                    var props = channel.CreateBasicProperties();
                    props.ReplyTo = replyQueueName;
                    props.CorrelationId = corrId;
                    
                    //encrypt password with SHA256Cng algorithm
                    using (var sha = new SHA256Cng())
                    {
                        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(authRequest.Password));
                        authRequest.Password = null;
                        var stringBuilder = new StringBuilder();
                        foreach (var x in hash)
                            stringBuilder.Append(String.Format("{0:x2}", x));
                        authRequest.Password = stringBuilder.ToString();
                    }

                    var messageBytes = authRequest.Serialize(); //message forward login and password
                    channel.BasicPublish("", "loginServer", props, messageBytes);

                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        if (ea.BasicProperties.CorrelationId == corrId)
                            return ea.Body.DeserializeAuthResponse();
                    }
                }
            }
        }

        private IConnectionFactory _factory;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
    }
}
