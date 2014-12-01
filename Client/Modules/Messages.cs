using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class Messages
    {
        public void SendMessage (MessageReq message)
        {
            var factory = new ConnectionFactory() {HostName = Const.HostName};
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic", true);
                    var body = message.Serialize();
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);
                    var routingKey = message.Recipient;
                    channel.BasicPublish("messages", routingKey, properties, body);
                    //(to determine)Server receive message and send it to correct user and save in database
                    //channel.BasicPublish("messages", "Server", properties, body);
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
                    var response = new MessageResponse();
                    response.Attachment = message.Attachment;
                    response.Message = message.Message;
                    response.Recipient = message.Login;
                    response.SendTime = message.SendTime;
                    response.Status = Status.OK;
                    return response;
                }
            }
        }

        /*public MessageResponse ReceiveMessageHistory(string yourLogin, string interlocutor)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("messages", "topic", true);
                    var queueName = channel.QueueDeclare();
                    channel.QueueBind(queueName, "messages", yourLogin);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = body.DeserializeMessageResponse();
                    return message;
                }
            }
        }*/
    }
}
