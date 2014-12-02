using System;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using System.Text;
using Common;
using RabbitMQ.Client;

namespace Client.Modules
{
    public class Login
    {
        public AuthResponse LoginAuthRequestResponse(AuthRequest authRequest)
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
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
        
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
    }
}
