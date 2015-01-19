using System.Linq.Expressions;
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
            scope = _container.BeginLifetimeScope();
        }

        public PresenceStatusNotification ListeningPresenceStatus()
        {
            PresenceStatusNotification user;
            try
            {
                var response = scope.Resolve<IPresenceStatus>();
                user = response.ReceiveUserPresenceStatus();
                return user;
            }
            catch { }
            return null;

        }

        public MessageResponse ListeningMessages()
        {
            MessageResponse messageResponse;
            try
            {
                var response = scope.Resolve<IMessages>();
                messageResponse = response.ReceiveMessage();
                return messageResponse;
            }
            catch { }
            return null;
        }

        public ActivityResponse ListeningActivity()
        {
            ActivityResponse activityResponse;
            try
            {
                var response = scope.Resolve<IActivity>();
                activityResponse = response.ActivityResponse();
                return activityResponse;
            }
            catch { }
            return null;
        }

        private IContainer _container;
        private const int Timeout = 100;
        private ILifetimeScope scope;
    }
}
