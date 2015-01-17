using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;
using System.Threading;

namespace Client
{
    public class Listening
    {
        public Listening()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            builder.RegisterType<Modules.Messages>().As<IMessages>();
            builder.RegisterType<Modules.Activity>().As<IActivity>();
            _container = builder.Build();
        }

        public PresenceStatusNotification ListeningPresenceStatus()
        {
            PresenceStatusNotification user;
            using (var scope = _container.BeginLifetimeScope())
            {
                var response = scope.Resolve<IPresenceStatus>();
                user = response.ReceiveUserPresenceStatus(Timeout);
            }
            return user;

        }

        public MessageResponse ListeningMessages()
        {
            MessageResponse messageResponse;
            using (var scope = _container.BeginLifetimeScope())
            {
                var response = scope.Resolve<IMessages>();
                messageResponse = response.ReceiveMessage(Timeout);
            }
            return messageResponse;
        }

        public ActivityResponse ListeningActivity()
        {
            ActivityResponse activityResponse;
            using (var scope = _container.BeginLifetimeScope())
            {
                var response = scope.Resolve<IActivity>();
                activityResponse = response.ActivityResponse(Timeout);
            }
            return activityResponse;
        }

        private IContainer _container;
        private const int Timeout = 200;
    }
}
