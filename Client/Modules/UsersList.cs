using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class UsersList
    {
        public UsersList()
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare();
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(replyQueueName, true, consumer);
        }
        public UserListResponse UserListReqResponse(string login)
        {
            var userListReq = new UserListReq();
            var corrId = Guid.NewGuid().ToString();
            var props = channel.CreateBasicProperties();
            props.ReplyTo = replyQueueName;
            props.CorrelationId = corrId;
            userListReq.Login = login;
            var messageBytes = userListReq.Serialize();//message forward login and password
            channel.BasicPublish("", "UsersListServer", props, messageBytes);
            while (true)
            {
                var ea = consumer.Queue.Dequeue();
                if (ea.BasicProperties.CorrelationId == corrId)
                {
                    return (ea.Body).DeserializeUserListResponse();
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
