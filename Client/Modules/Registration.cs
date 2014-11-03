using System;
using System.Text;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    //example
    public class Registration
    {
        public Registration()
        {
            var factory = new ConnectionFactory {HostName = Const.HostName, Port = AmqpTcpEndpoint.DefaultAmqpSslPort};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare();
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(replyQueueName, true, consumer);
        }
        //this method returns result of Registration.register from server
        public CreateUserResponse registration(string login, string password)
        {
            createUserReq = new CreateUserReq();
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            createUserReq.Login = login;
            createUserReq.Password = password;

            var messageBytes = createUserReq.Serialize();//message forward login and password
            channel.BasicPublish("", "registrationServer", props, messageBytes);

            while (true)
            {
                var ea = consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    return (CreateUserResponse)(ea.Body).Deserialize();
                }
            }
        }
        public void Close()
        {
            connection.Close();
        }

        private static string message = "fg";
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
        private CreateUserReq createUserReq;
    }
}
