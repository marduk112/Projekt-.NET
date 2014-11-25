using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Client.Modules
{
    public class Activity
    {
        public void ActivityReq(string login, bool isWriting, string recipient)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("activity", "topic");
                    var activityReq = new ActivityReq();
                    activityReq.IsWriting = isWriting;
                    activityReq.Login = login;
                    activityReq.Recipient = recipient;
                    var body = activityReq.Serialize();
                    channel.BasicPublish("activity", "Activity."+recipient, null, body);
                }
            }
        }

        public ActivityResponse ActivityResponse(string login)
        {
            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("activity", "topic");
                    var queueName = channel.QueueDeclare();
                    channel.QueueBind(queueName, "activity", "Activity." + login);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var message = body.DeserializeActivityReq();
                    var activityResponse = new ActivityResponse();
                    activityResponse.Recipient = message.Login;
                    activityResponse.Status = Status.OK;
                    activityResponse.IsWriting = message.IsWriting;
                    return activityResponse;
                }
            }
        }
    }
}
