using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Server.Modules
{
    //example
    public static class Registration
    {
        public static void registration()
        {
            var factory = new ConnectionFactory() {HostName = Const.HostName};
            //factory.Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("registrationServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("registrationServer", false, consumer);

                    while (true)
                    {
                        CreateUserResponse response = new CreateUserResponse();
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        try
                        {
                            message = (CreateUserReq)body.Deserialize();
                            response = correctRegister();
                        }
                        catch (Exception e)
                        {
                            response = incorrectRegister("Incorrect registration");
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

        private static CreateUserResponse correctRegister()
        {
            CreateUserResponse createUserResponse;
            XDocument xmlDoc;
            //load users list from XML file
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
                where login != null
                //let password = user.Element("Password")
                //where password != null
                select new
                        {
                            Login = login.Value,
                            //Password = password.Value,
                        };
            //Is user exist
            bool isExist = users.Any(user => user.Login.Equals(message.Login));
            if (isExist)
            {
                createUserResponse = new CreateUserResponse();
                createUserResponse.Status = Status.NotAuthenticated;
                createUserResponse.Message = "User with login " + message.Login + "  exist";
            }
            else
            {
                createUserResponse = new CreateUserResponse();
                createUserResponse.Status = Status.OK;
                createUserResponse.Message = "Successful registration";
                var xElement = xmlDoc.Element("Users");
                if (xElement != null)
                    xElement.Add(new XElement("User", new XElement("Login", message.Login),
                        new XElement("Password", message.Password)));
                else
                    return incorrectRegister("Error");
                xmlDoc.Save(Const.FileNameToRegAndLogin);
            }
            xmlDoc = null;
            return createUserResponse;
        }

        private static CreateUserResponse incorrectRegister(string error)
        {
            CreateUserResponse createUserResponse = new CreateUserResponse();
            createUserResponse.Status = Status.Error;
            createUserResponse.Message = error;
            return createUserResponse;
        }

        private static CreateUserReq message;
    }
}
