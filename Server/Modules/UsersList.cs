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
            _usersPresenceStatus.CollectionChanged += _usersPresenceStatus_CollectionChanged;
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
                            message = body.DeserializeUserListReq();
                            response = GetUserList();
                        }
                        catch (Exception e)
                        {
                            response = ErrorUserListResponseResponse("Error");
                        }
                        finally
                        {
                            var responseBytes = response.Serialize();
                            channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                    }
        }
        //send friends list with presence status(from database)
        public void friendsList()
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare("FriendsListServer", false, false, false, null);
            channel.BasicQos(0, 1, false);
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume("FriendsListServer", false, consumer);
            while (true)
            {
                var response = new UserListResponse();
                channel.ExchangeDeclare("FriendsListServer", "topic", true);
                var queueName = channel.QueueDeclare();
                //channel.QueueBind(queueName, "FriendsListServer", userListReq.Login);
                channel.BasicConsume(queueName, true, consumer);
                var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                var body = ea.Body;
                try
                {
                    message = body.DeserializeUserListReq();
                    response = GetFriendsList(message.Login);
                }
                catch (Exception e)
                {
                    response = ErrorUserListResponseResponse("Error");
                }
                finally
                {
                    var responseBytes = response.Serialize();
                    channel.BasicPublish("UsersStatus", "Server", null, responseBytes);
                }
            }
        }
        //get friends and their presence status for user with nick=nick
        private UserListResponse GetFriendsList(string nick)
        {
            throw new NotImplementedException();
        }
        private UserListResponse GetUserList()
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
                        where !login.IsEmpty && login.Value != message.Login
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
        private UserListResponse ErrorUserListResponseResponse(string error)
        {
            var userListResponse = new UserListResponse();
            userListResponse.Status = Status.Error;
            userListResponse.Message = error;
            return userListResponse;
        }

        private void GetUserPresenceStatus()
        {
            var channel = connection.CreateModel();
            channel.ExchangeDeclare("UsersStatus", "topic");
            var consumer = new QueueingBasicConsumer(channel);
            var queueName = channel.QueueDeclare();
            channel.BasicConsume(queueName, true, consumer);
            var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
            var body = ea.Body;
            var message = body.DeserializeUser();
            _usersPresenceStatus.Add(message.Login, message.Status);
        }

        //topic is a user nick whose presence status has changed
        private void _usersPresenceStatus_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove)
            {
                var channel = connection.CreateModel();
                channel.ExchangeDeclare("UsersStatus", "topic");
                var queueName = channel.QueueDeclare();
                foreach (var user in _usersPresenceStatus.ReturnDictionary().Keys)
                {
                    channel.QueueBind(queueName, "UsersStatus", user, null);
                    var temp = new User {Login = user, Status = _usersPresenceStatus.ReturnDictionary()[user]};
                    channel.BasicPublish("UsersStatus", user, null, temp.Serialize());
                    if (temp.Status == PresenceStatus.Offline)
                    {
                        channel.QueueUnbind(queueName, "UsersStatus", user, null);
                        _usersPresenceStatus.Remove(temp.Login);
                    }
                }
            }
        }

        private UserListReq message;
        private IConnection connection;
        private IUsersListDataAccess _dataAccess;
        private UsersPresenceStatus _usersPresenceStatus = new UsersPresenceStatus();
    }
}
