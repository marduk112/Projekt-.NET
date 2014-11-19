using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;

namespace Server.Modules
{
    public static class UsersList
    {
        public static void usersList()
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            //factory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
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
            }
        }
        private static UserListResponse GetUserList()
        {
            var userListResponse = new UserListResponse();
            XDocument xmlDoc;
            //load users list from XML file
            if (!Directory.Exists("Databases"))
                Directory.CreateDirectory("Databases");
            if (!File.Exists(Const.FileNameToRegAndLogin))
            {
                File.Create(Const.FileNameToRegAndLogin);
                xmlDoc = XDocument.Load(Const.FileNameToRegAndLogin);
                xmlDoc.Add(new XElement("Users"));
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
        private static UserListResponse ErrorUserListResponseResponse(string error)
        {
            var userListResponse = new UserListResponse();
            userListResponse.Status = Status.Error;
            userListResponse.Message = error;
            return userListResponse;
        }

        private static UserListReq message;
    }
}
