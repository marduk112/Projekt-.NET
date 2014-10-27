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
            var factory = new ConnectionFactory() {HostName = "localhost"};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("registrationServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("registrationServer", false, consumer);
                    Console.WriteLine(" [x] Awaiting RPC requests");

                    while (true)
                    {
                        bool response = false;
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        try
                        {
                            message = (String)JSONPayloadSerializer.Deserialize(body);
                            response = register();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(" [.] " + e.Message);
                            response = false;
                        }
                        finally
                        {
                            var responseBytes = Encoding.UTF8.GetBytes(response.ToString());
                            channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
                            channel.BasicAck(ea.DeliveryTag, false);
                        }
                    }
                }
            }
        }

        private static bool register()
        {
            //XML to LINQ
            return false;
        }

        private static string message;
    }
}
