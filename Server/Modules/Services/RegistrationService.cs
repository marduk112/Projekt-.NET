using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;
using Server.DataModels;
using Server.Data_Access;
using SQLite;

namespace Server.Modules
{
    class RegistrationService : IServicable
    {
        private static string ServiceName = "regServer";
        private CreateUserReq message;
        private volatile bool _work;
        private static string logMsg = " attempted to register. Result: ";


        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Const.ClientExchange, "topic");
                channel.QueueDeclare(ServiceName, false, false, false, null);
                channel.BasicQos(0, 1, false);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(ServiceName, false, consumer);

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
                        Logger.serviceLog(response, message, logMsg);
                        var responseBytes = response.Serialize();
                        channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                        channel.BasicAck(ea.DeliveryTag, false);
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

            var db = new Database();
            try
            {
                db.RegisterUser(message.Login, message.Password);
                createUserResponse = new CreateUserResponse();
                createUserResponse.Status = Status.OK;
                createUserResponse.Message = "Successful registration";
            }
            catch (SQLiteException)
            {
                createUserResponse = incorrectRegister("User with login " + message.Login + " exist");   
            }
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
