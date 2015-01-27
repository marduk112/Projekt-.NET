using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using Server.Data_Access;
using Server.Modules.Interfaces;

namespace Server.Modules.Services
{
    class PresenceStatusService : IServicable
    {
        private static string ServiceName = "PresenceStatusServer";
        private PresenceStatusRequest message;
        private volatile bool _work;
        private static string logMsg = " attempted to change status. Result: ";

        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
                var queueName = channel.QueueDeclare();
                channel.QueueBind(queueName, Const.ClientExchange, ServiceName);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queueName, false, consumer);

                while (_work)
                {
                    var db = new Database();
                    var response = new PresenceStatusResponse();
                    var ea = consumer.Queue.Dequeue();
                    var body = ea.Body;
                    try
                    {
                        var user = db.QueryUser(message.Login);
                        message = body.DeserializePresenceStatusRequest();
                        db.ChangeUserStatus(ref user, message.PresenceStatus);
                        response.Status = Status.OK;
                        response.Message = "Status changed successfully";
                    }
                    catch (Exception e)
                    {
                        response.Status = Status.Error;
                        response.Message = "Status change failure";
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        Logger.serviceLog(response, message, logMsg);
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }

        public void Stop()
        {
            _work = false;
        }
    }
}
