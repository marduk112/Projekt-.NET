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
        public Registration()
        {
            var factory = new ConnectionFactory {HostName = Const.HostName};
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare();
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(replyQueueName, true, consumer);
        }
        //this method returns result of Registration.register from server
        public CreateUserResponse registration(string login, string password)
        {
            var createUserReq = new CreateUserReq();
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;

            createUserReq.Login = login;
            //encrypt password with SHA256 algorithm
            using (var sha1 = new SHA256Managed())
            {
                createUserReq.Password = Encoding.UTF8.GetString(sha1.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }

            var messageBytes = createUserReq.Serialize();//message forward login and password
            channel.BasicPublish("", "regLogServer", props, messageBytes);
           
            var ea = consumer.Queue.Dequeue();
            while (true)
            {
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    return (ea.Body).Deserialize() as CreateUserResponse;
                }
            }
        }
        public void CloseConnection()
        {
            connection.Close();
        }
        
        private IConnection connection;
        private IModel channel;
        private string replyQueueName;
        private QueueingBasicConsumer consumer;
    }
}
