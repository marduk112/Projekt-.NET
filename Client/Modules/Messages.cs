using System;
using System.IO;
using System.ServiceModel.Channels;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl.v0_9;
using ServiceStack.Text;

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
                    channel.ExchangeDeclare("messages", "topic", true);
                    var routingKey = recipient;
                    var body = message.Serialize();
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);
                    channel.BasicPublish("messages", routingKey, properties, body);
                }
            }
        }
        //every user has queue for message response
        public MessageResponse ReceiveMessage(string login)
        {
            var factory = new ConnectionFactory() {HostName = Const.HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic", true);
                    var queueName = channel.QueueDeclare();
                    channel.QueueBind(queueName, "messages", login);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = body.DeserializeMessageReq();
                    var messageResponse = new MessageResponse();
                    messageResponse.Message = message.Message;
                    messageResponse.Attachment = message.Attachment;
                    messageResponse.Recipient = message.Login;
                    messageResponse.SendTime = message.SendTime;
                    messageResponse.Status = Status.OK;
                    return messageResponse;
                }
            }
        }

        
    }
}
