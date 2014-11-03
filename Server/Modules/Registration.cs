using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                            //Console.WriteLine(" [.] " + e.Message);
                            response = incorrectRegister();
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
            //XML to LINQ
            CreateUserResponse createUserResponse = new CreateUserResponse();
            createUserResponse.Status = Status.OK;
            return createUserResponse;
        }

        private static CreateUserResponse incorrectRegister()
        {
            CreateUserResponse createUserResponse = new CreateUserResponse();
            createUserResponse.Status = Status.Error;
            createUserResponse.Message = "Incorrect register";
            return createUserResponse;
        }

        private static CreateUserReq message;
    }
}
