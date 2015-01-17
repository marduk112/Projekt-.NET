using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using RabbitMQ.Client;
using Server.Data_Access;
using SQLite;

namespace Server.Modules
{
    class UsersListService : IServicable
    {
        private static string ServiceName = "UsersListServer";
        private bool _work;
        private static UserListReq message;
        public void Start()
        {
            _work = true;
            var db = new Database();

            var factory = new ConnectionFactory { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(ServiceName, false, false, false, null);
                    channel.BasicQos(0, 1, false);
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
                                Message = "Lista użytkowników", 
                                Status = Status.OK, 
                                Users = (List<Common.User>) db.QueryAllUsers() 
                            };
                        }
                        catch (Exception e)
                        {
                            response = new UserListResponse
                            {
                                Message = "Nie udało się pobrać listy użytkowników",
                                Status = Status.Error,
                                Users = null
                            };
                        }
                        finally
                        {
                            log(response);
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
        private void log(UserListResponse response)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToLongTimeString());
            sb.Append(" - ");
            sb.Append(message.Login);
            sb.Append(" attempted to fetch users list. Result: ");
            sb.Append(response.Status);
            Console.WriteLine(sb.ToString());
        }
    }
}
