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
            //encrypt password with SHA256Cng algorithm
            using (var sha = new SHA256Cng())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                createUserReq.Password = string.Empty;
                foreach (var x in hash)
                {
                    createUserReq.Password += String.Format("{0:x2}", x);
                }
            }
            var messageBytes = createUserReq.Serialize();//message forward login and password
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
