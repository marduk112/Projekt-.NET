using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Server.Modules
{
    public class MessageHistory
    {
        public MessageHistory(IMessageHistoryDataAccess dataAccess)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            _dataAccess = dataAccess;
        }

        public void ReceiveMessages()
        {
            channel.ExchangeDeclare("messages", "topic", true);
            var queueName = channel.QueueDeclare();
            channel.QueueBind(queueName, "messages", "Server");
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            while (true)
            {
                var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                var body = ea.Body;
                var message = body.DeserializeMessageReq();
                //dataAccess.SaveMessage(message);
                /*var response = new MessageResponse();
                response.Attachment = message.Attachment;
                response.Message = message.Message;
                response.Recipient = message.Login;
                response.SendTime = message.SendTime;
                var properties = channel.CreateBasicProperties();
                properties.SetPersistent(true);
                channel.BasicPublish("messages", message.Recipient, properties, response.Serialize());*/
            }
        }

        public void CloseConnection()
        {
            connection.Close();
        }

        private IConnection connection;
        private IModel channel;
        private IMessageHistoryDataAccess _dataAccess;
    }
}
