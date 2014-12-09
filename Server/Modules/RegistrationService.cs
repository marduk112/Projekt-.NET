using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;

namespace Server.Modules
{
    class RegistrationService : IServicable
    {
        private bool _work;
        private CreateUserReq message;

        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("regServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("regServer", false, consumer);

                    while (_work)
                    {
                        var response = new CreateUserResponse();
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        try
                        {
                            message = body.DeserializeCreateUserReq();
                            response = correctRegister();
                        }
                        catch (Exception e)
                        {
                            response = incorrectRegister("Incorrect registration");
                            Console.WriteLine(e.Message);
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

        public void Stop()
        {
            _work = false;
        }

        private CreateUserResponse correctRegister()
        {
            CreateUserResponse createUserResponse;
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
                        where login != null
                        select new
                        {
                            Login = login.Value,
                        };
            //Is user exist
            bool isExist = users.Any(user => user.Login.Equals(message.Login));
            if (isExist)
            {
                return incorrectRegister("User with login " + message.Login + " exist");
            }
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
            return createUserResponse;
        }

        private CreateUserResponse incorrectRegister(string error)
        {
            var createUserResponse = new CreateUserResponse();
            createUserResponse.Status = Status.Error;
            createUserResponse.Message = error;
            return createUserResponse;
        }
    }
}
