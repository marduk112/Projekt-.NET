using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using Server.Data_Access;
using SQLite;
using Server.Modules.Interfaces;

namespace Server.Modules.Services
{
    class AddFriendService : IServicable
    {
        private static string ServiceName = "AddFriendServer";
        private AddFriendReq message;
        private volatile bool _work;
        private static string logMsg = " attempted to add friend. Result: ";


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
                    var response = new AddFriendResponse();
                    var ea = consumer.Queue.Dequeue();

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        message = body.DeserializeAddFriendReq();
                        response = correctAddFriend();
                    }
                    catch (Exception e)
                    {
                        response = incorrectAddFriend("Incorrect friend");
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

        private AddFriendResponse correctAddFriend()
        {
            AddFriendResponse addFriendResponse;

            var db = new Database();
            try
            {
                db.AddFriend(message.Login, message.FriendLogin);
                addFriendResponse = new AddFriendResponse();
                addFriendResponse.Status = Status.OK;
                addFriendResponse.Message = "Successful added";
            }
            catch (DataException)
            {
                addFriendResponse = incorrectAddFriend("User with login " + message.FriendLogin + " is already a friend");
            }
            return addFriendResponse;
        }

        private AddFriendResponse incorrectAddFriend(string error)
        {
            var addFriendResponse = new AddFriendResponse();
            addFriendResponse.Status = Status.Error;
            addFriendResponse.Message = error;
            return addFriendResponse;
        }
    }
}
