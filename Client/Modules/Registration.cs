using System;
using System.Security.Cryptography;
using System.Text;
using Common;
using RabbitMQ.Client;

namespace Client.Modules
{
    //example
    public class Registration
    {
        public Registration(IConnectionFactory factory)
        {
            _factory = factory;
        }
        //this method returns result of Registration.register from server
        public CreateUserResponse registration(CreateUserReq createUserReq)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var replyQueueName = channel.QueueDeclare();
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(replyQueueName, true, consumer);
                    var corrId = Guid.NewGuid().ToString();
                    var props = channel.CreateBasicProperties();
                    props.ReplyTo = replyQueueName;
                    props.CorrelationId = corrId;
                    
                    //encrypt password with SHA256Cng algorithm
                    using (var sha = new SHA256Cng())
                    {
                        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(createUserReq.Password));
                        createUserReq.Password = null;
                        var stringBuilder = new StringBuilder();
                        foreach (var x in hash)
                            stringBuilder.Append(String.Format("{0:x2}", x));
                        createUserReq.Password = stringBuilder.ToString();
                    }
                    var messageBytes = createUserReq.Serialize(); //message forward login and password
                    channel.BasicPublish("", "regServer", props, messageBytes);

                    var ea = consumer.Queue.Dequeue();
                    while (true)
                    {
                        if (ea.BasicProperties.CorrelationId == corrId)
                        {
                            return ea.Body.DeserializeCreateUserResponse();
                        }
                    }
                }
            }
        }

        private IConnectionFactory _factory;
    }
}
