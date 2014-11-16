using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;

namespace Server.Modules
{
    public class Login
    {
        public static void login()
        {
            var factory = new ConnectionFactory {HostName = Const.HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("regLogServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("regLogServer", false, consumer);

                    while (true)
                    {
                        var response = new AuthResponse();
                        var ea = consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;
                        try
                        {
                            message = body.Deserialize() as AuthRequest;
                            response = correctAuthentication();
                        }
                        catch (Exception e)
                        {
                            response = IncorrectAuthentication("Error");
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

        private static AuthResponse correctAuthentication()
        {
            //XML to LINQ
            XDocument xmlDoc;
            var authResponse = new AuthResponse();
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
                        where !login.IsEmpty
                        let password = user.Element("Password")
                        where !password.IsEmpty
                        select new
                        {
                            Login = login.Value,
                            Password = password.Value,
                        };
            //Is authenticated
            bool isAuth = users.Any(user => user.Login.Equals(message.Login) && user.Password.Equals(message.Password));
            if (isAuth)
            {
                authResponse.Status = Status.OK;
                authResponse.Message = "Successful authentication";
                authResponse.IsAuthenticated = true;
            }
            else
            {
                IncorrectAuthentication("Wrong login or password");
            }
            xmlDoc = null;
            return authResponse;
        }

        private static AuthResponse IncorrectAuthentication(string error)
        {
            var authResponse = new AuthResponse();
            authResponse.Status = Status.Error;
            authResponse.Message = error;
            authResponse.IsAuthenticated = false;
            return authResponse;
        }

        private static AuthRequest message;
    }
}
