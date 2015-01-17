using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;

namespace Client.Modules
{
    public class FriendsList : IFriendsList
    {
        public FriendsList(IConnectionFactory factory)
        {
            _factory = factory;
        }
        public void AddFriend(AddFriendReq addFriendReq)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    var replyQueueName = channel.QueueDeclare();
                    var corrId = Guid.NewGuid().ToString();
                    var props = channel.CreateBasicProperties();
                    channel.QueueDeclare("AddFriendServer", false, false, false, null);
                    var messageBytes = addFriendReq.Serialize();
                    props.ReplyTo = replyQueueName;
                    props.CorrelationId = corrId;
                    channel.BasicPublish("", "AddFriendServer", props, messageBytes);
                   
                }
            }
        }

        public void DeleteFriend(DeleteFriendReq deleteFriendReq)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("DeleteFriendServer", false, false, false, null);
                    var messageBytes = deleteFriendReq.Serialize();
                    channel.BasicPublish("", "DeleteFriendServer", null, messageBytes);
                }
            }
        }

        private IConnectionFactory _factory;
    }
}
