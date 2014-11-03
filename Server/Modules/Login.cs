using System;
using Common;
using RabbitMQ.Client;

namespace Server.Modules
{
    public class Login
    {
        public static void login()
        {
            var factory = new ConnectionFactory {HostName = Const.HostName, Port = AmqpTcpEndpoint.DefaultAmqpSslPort};
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
                        AuthResponse response = new AuthResponse();
                        var ea = consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;
                        try
                        {
                            AuthRequest message = (AuthRequest)body.Deserialize();
                            response = correctAuthentication();
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine(" [.] " + e.Message);
                            response = incorrectAuthentication();
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
            AuthResponse authResponse = new AuthResponse();
            authResponse.Status = Status.OK;
            return authResponse;
        }

        private static AuthResponse incorrectAuthentication()
        {
            AuthResponse authResponse = new AuthResponse();
            authResponse.Status = Status.Error;
            authResponse.Message = "Incorrect register";
            return authResponse;
        }
    }
}
