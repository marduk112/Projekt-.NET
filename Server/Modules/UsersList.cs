using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using Common.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Server.Data_Access;

namespace Server.Modules
{
    public class UsersList
    {
        public UsersList(IUsersListDataAccess dataAccess)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            connection = factory.CreateConnection();
            _dataAccess = dataAccess;
        }

        //send all users lisy
        public void usersList()
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare("UsersListServer", false, false, false, null);
            channel.BasicQos(0, 1, false);
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume("UsersListServer", false, consumer);
            while (true)
            {
                var response = new UserListResponse();
                var ea = consumer.Queue.Dequeue();
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                try
                {
                    var message = body.DeserializeUserListReq();
                    response = GetUserList(message.Login);//connect with database
                }
                catch
                {
                    response = ErrorUserListResponse("Error");
                }
                finally
                {
                    var responseBytes = response.Serialize();
                    channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
        }
        //send friends list with presence status(from _userPresenceStatus)
        public void FriendsList()
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare("FriendsListServer", false, false, false, null);
            channel.BasicQos(0, 1, false);
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume("FriendsListServer", false, consumer);
            while (true)
            {
                var response = new UserListResponse();
                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                var body = ea.Body;
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;
                try
                {
                    var message = body.DeserializeUserListReq();
                    response = GetFriendsList(message.Login);//connect with database
                }
                catch
                {
                    response = ErrorUserListResponse("Error");
                }
                finally
                {
                    var responseBytes = response.Serialize();
                    channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
            }
        }

        //to determine
        public void ReceiveUserPresenceStatus()
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("UsersStatus", "topic");
            var queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, "UsersStatus", "Server", null);
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            while (true)
            {
                var ea = consumer.Queue.Dequeue();
                var message = ea.Body;
                var userPresenceStatus = message.DeserializeUser();
                channel.QueueBind(queueName, "UsersStatus", userPresenceStatus.Login);
                if (_usersPresenceStatus.ReturnDictionary().ContainsKey(userPresenceStatus.Login) && userPresenceStatus.Status == PresenceStatus.Offline)
                    _usersPresenceStatus.Remove(userPresenceStatus.Login);
                else
                    _usersPresenceStatus.Add(userPresenceStatus.Login, userPresenceStatus.Status);
                channel.BasicPublish("UsersStatus", userPresenceStatus.Login, null, _usersPresenceStatus.GetUser(userPresenceStatus.Login).Serialize());
            }
        }

        //get friends and their presence status for user with nick=nick(i.e from _userPresenceStatus and get nick's from database)
        private UserListResponse GetFriendsList(string nick)
        {
            throw new NotImplementedException();
        }
        private UserListResponse GetUserList(string nick)
        {
            var userListResponse = new UserListResponse();
            XDocument xmlDoc;
            //load users list from XML file
            if (!Directory.Exists("Databases"))
                Directory.CreateDirectory("Databases");
            if (!File.Exists(Const.FileNameToRegAndLogin))
            {
                File.AppendAllText(Const.FileNameToRegAndLogin, "<Users>\n</Users>");
                xmlDoc = XDocument.Load(Const.FileNameToRegAndLogin);
            }
            else
                xmlDoc = XDocument.Load(Const.FileNameToRegAndLogin);
            //XML to LINQ
            var users = from user in xmlDoc.Descendants("User")
                        let login = user.Element("Login")
                        where !login.IsEmpty && login.Value != nick
                        select new
                        {
                            Login = login.Value,
                        };
            foreach (var user in users.Select(login => new User()))
            {
                user.Login = user.Login;
                userListResponse.Users.Add(user);
                userListResponse.Status = Status.OK;
                userListResponse.Message = "Correct downloaded users list";
            }
            return userListResponse;
        }
        private UserListResponse ErrorUserListResponse(string error)
        {
            var userListResponse = new UserListResponse();
            userListResponse.Status = Status.Error;
            userListResponse.Message = error;
            return userListResponse;
        }
       
        private IConnection connection;
        private IUsersListDataAccess _dataAccess;
        private UsersPresenceStatus _usersPresenceStatus = new UsersPresenceStatus();
    }
}
