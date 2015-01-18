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
                    channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);
                    var messageBytes = addFriendReq.Serialize();
                    channel.BasicPublish(Const.ClientExchange, "AddFriendServer", properties, messageBytes);
                }
            }
        }

        public void DeleteFriend(DeleteFriendReq deleteFriendReq)
        {
            using (var connection = _factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);
                    var messageBytes = deleteFriendReq.Serialize();
                    channel.BasicPublish(Const.ClientExchange, "DeleteFriendServer", properties, messageBytes);
                }
            }
        }

        private IConnectionFactory _factory;
    }
}
