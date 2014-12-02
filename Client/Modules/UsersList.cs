using System;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class UsersList
    {
        public UserListResponse UserListReqResponse(UserListReq userListReq)
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
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
                    var messageBytes = userListReq.Serialize(); //message forward login and password
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
            }
        }

        public UserListResponse GetFriendsListWithPresenceStatus(UserListReq userListReq)
        {
            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
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
                    var messageBytes = userListReq.Serialize(); //message forward login and password
                    channel.BasicPublish("", "FriendsListServer", props, messageBytes);
                    while (true)
                    {
                        var ea = consumer.Queue.Dequeue();
                        if (ea.BasicProperties.CorrelationId == corrId)
                        {
                            return (ea.Body).DeserializeUserListResponse();
                        }
                    }
                }
            }
        }
    }
}
