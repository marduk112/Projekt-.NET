using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using Server.Data_Access;

namespace Server.Modules
{
    class FriendListService : IServicable
    {
        private bool _work;
        private static string ServiceName = "FriendListServer";
        private static UserListReq message;
        private static string logMsg = " attempted to fetch friends list. Result: ";
        public void Start()
        {
            _work = true;
            var db = new Database();

            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //channel.ExchangeDeclare(Const.ClientExchange, "topic");
                channel.QueueDeclare(ServiceName, false, false, false, null);
                channel.BasicQos(0, 1, false);
                //channel.QueueBind(ServiceName, Const.ClientExchange, Const.ServerFriendListRequestRoute);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(ServiceName, false, consumer);

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
                        response = new UserListResponse
                        {
                            Message = "Lista znajmoych",
                            Status = Status.OK,
                            Users = db.QueryAllFriends(message.Login)
                        };
                    }
                    catch
                    {
                        response = new UserListResponse
                        {
                            Message = "Nie udało się pobrać listy znajomych",
                            Status = Status.Error,
                            Users = null
                        };
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
    }
}
