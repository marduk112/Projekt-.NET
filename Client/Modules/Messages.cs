using System;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl.v0_9;

namespace Client.Modules
{
    public class Messages
    {
        public void SendMessage(string sender, string recipient, MessageReq message)
        {
            var factory = new ConnectionFactory() {HostName = Const.HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic");
                    channel.QueueDeclare("Messages", true, false, false, null);
                    var routingKey = sender + "." + recipient;
                    var body = message.Serialize();
                    channel.BasicPublish("messages", routingKey, null, body);
                }
            }
        }

        public MessageResponse ReceiveMessage(string login)
        {
            var factory = new ConnectionFactory() {HostName = Const.HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic");
                    var queueName = "Messages";
                    channel.QueueBind(queueName, "messages", "*." + login + ".*");
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var message = body.Deserialize() as MessageReq;
                        var messageResponse = new MessageResponse();
                        messageResponse.Message = message.Message;
                        messageResponse.Attachment = message.Attachment;
                        messageResponse.Recipient = message.Recipient;
                        messageResponse.SendTime = message.SendTime;
                        messageResponse.Status = Status.OK;
                    return messageResponse;
                }
            }
        }
    }
}
