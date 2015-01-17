using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using Server.Data_Access;

namespace Server.Modules.Services
{
    class DeleteFriendService : IServicable
    {
        private static string ServiceName = "DeleteFriendServer";
        private DeleteFriendReq message;
        private volatile bool _work;
        private static string logMsg = " attempted to delete friend. Result: ";


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
                    var response = new DeleteFriendResponse();
                    var ea = consumer.Queue.Dequeue();

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        message = body.DeserializeDeleteFriendReq();
                        response = correctDeleteFriend();
                    }
                    catch (Exception e)
                    {
                        response = incorrectDeleteFriend("Incorrect friend");
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

        private DeleteFriendResponse correctDeleteFriend()
        {
            DeleteFriendResponse deleteFriendResponse;

            var db = new Database();
            try
            {
                db.DeleteFriend(message.Login, message.FriendLogin);
                deleteFriendResponse = new DeleteFriendResponse();
                deleteFriendResponse.Status = Status.OK;
                deleteFriendResponse.Message = "Successful deleted";
            }
            catch (DataException)
            {
                deleteFriendResponse = incorrectDeleteFriend("User with login " + message.FriendLogin + " is not your friend");
            }
            return deleteFriendResponse;
        }

        private DeleteFriendResponse incorrectDeleteFriend(string error)
        {
            var deleteFriendResponse = new DeleteFriendResponse();
            deleteFriendResponse.Status = Status.Error;
            deleteFriendResponse.Message = error;
            return deleteFriendResponse;
        }
    }
}
