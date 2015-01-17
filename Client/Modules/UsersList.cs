using System;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class UsersList : IUsersList
    {
        public UsersList(IConnectionFactory factory)
        {
            _factory = factory;
        }
        public UserListResponse UserListReqResponse(UserListReq userListReq)
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
                    var messageBytes = userListReq.Serialize(); //message forward login and password
                    channel.BasicPublish("", "FriendListServer", props, messageBytes);
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

        private IConnectionFactory _factory;
    }
}
