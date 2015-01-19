using Common;
using RabbitMQ.Client;
using Server.Data_Access;
using Server.Modules.Interfaces;
using Server.Modules.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Modules.Services
{
    class MessageService : IServicable
    {
        private static string ServiceName = "MessageServer";
        private MessageReq message;
        private volatile bool _work;
        private static string logMsg = " attempted to send message. Result: ";


        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(Const.ClientExchange, "topic", true);
                var queueName = channel.QueueDeclare();
                channel.QueueBind(queueName, Const.ClientExchange, Const.ServerMessageRequestRoute);
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queueName, false, consumer);

                while (_work)
                {
                    var response = new MessageResponse();
                    var ea = consumer.Queue.Dequeue();
                    var body = ea.Body;
                    try
                    {
                        message = body.DeserializeMessageReq();
                        response = correctSendMessage(message);
                    }
                    catch (Exception e)
                    {
                        response = incorrectSendMessage("Problem sending message");
                        Console.WriteLine(e.Message);
                    }
                    finally
                    {
                        Logger.serviceLog(response, message, logMsg);
                        var props = channel.CreateBasicProperties();
                        props.SetPersistent(true);
                        var routingKey = Const.ClientMessageNotificationRoute + response.Recipient;
                        channel.BasicPublish(Const.ClientExchange, routingKey, props, response.Serialize());
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }

        public void Stop()
        {
            _work = false;
        }

        private MessageResponse correctSendMessage(MessageReq message)
        {
            var date = DateTimeOffset.Now;
            
            /*var messageNotification = new MessageNotification();
            messageNotification.Sender = message.Login;
            messageNotification.Recipient = message.Recipient;
            messageNotification.Message = message.Message;
            messageNotification.Attachment = message.Attachment;
            messageNotification.SendTime = date;

            var messageSender = new MessageSender();
            messageSender.Send(messageNotification);*/

            var messageResponse = new MessageResponse
            {
                Status = Status.OK,
                Message = message.Message,
                Login = message.Login,
                Recipient = message.Recipient,
                Attachment = message.Attachment,
                SendTime = date
            };

            return messageResponse;
        }

        private MessageResponse incorrectSendMessage(string error)
        {
            var messageResponse = new MessageResponse();
            messageResponse.Status = Status.Error;
            messageResponse.Message = error;
            return messageResponse;
        }
    }
}
