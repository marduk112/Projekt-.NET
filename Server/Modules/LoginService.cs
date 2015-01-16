using System;
using System.Collections.Generic;
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

namespace Server.Modules
{
    class LoginService : IServicable
    {
        private bool _work;

        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("loginServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("loginServer", false, consumer);

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
                            message = body.DeserializeAuthRequest();
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

        public void Stop()
        {
            _work = false;
        }

        private static AuthResponse correctAuthentication()
        {
            var db = new Database();
            var authResponse = new AuthResponse();
            bool isAuth = db.LoginUser(message.Login, message.Password);
            if (isAuth)
            {
                authResponse.Status = Status.OK;
                authResponse.Message = "Successful authentication";
                authResponse.IsAuthenticated = true;
            }
            else
            {
                authResponse = IncorrectAuthentication("Wrong login or password");
            }
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
