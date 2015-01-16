﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common;
using RabbitMQ.Client;
using Server.DataModels;
using SQLite;

namespace Server.Modules
{
    class RegistrationService : IServicable
    {
        
        private CreateUserReq message;
        private volatile bool _work;


        public void Start()
        {
            _work = true;

            var factory = new ConnectionFactory() { HostName = Const.HostName };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare("regServer", false, false, false, null);
                    channel.BasicQos(0, 1, false);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume("regServer", false, consumer);

                    while (_work)
                    {
                        var response = new CreateUserResponse();
                        var ea = consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var props = ea.BasicProperties;
                        var replyProps = channel.CreateBasicProperties();
                        replyProps.CorrelationId = props.CorrelationId;

                        try
                        {
                            message = body.DeserializeCreateUserReq();
                            response = correctRegister();
                        }
                        catch (Exception e)
                        {
                            response = incorrectRegister("Incorrect registration");
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
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

        private CreateUserResponse correctRegister()
        {
            CreateUserResponse createUserResponse;

            var db = new Database();
            try
            {
                db.RegisterUser(message.Login, message.Password);
                createUserResponse = new CreateUserResponse();
                createUserResponse.Status = Status.OK;
                createUserResponse.Message = "Successful registration";
            }
            catch (SQLiteException e)
            {
                createUserResponse = incorrectRegister("User with login " + message.Login + " exist");   
            }
            return createUserResponse;
        }

        private CreateUserResponse incorrectRegister(string error)
        {
            var createUserResponse = new CreateUserResponse();
            createUserResponse.Status = Status.Error;
            createUserResponse.Message = error;
            return createUserResponse;
        }
    }
}
