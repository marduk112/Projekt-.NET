using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Client.Interfaces;
using Common;
using RabbitMQ.Client;

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
            _isRun = true;
        }

        public PresenceStatusNotification ListeningPresenceStatus()
        {
            if (!_isRun) return null;
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
            if (!_isRun) return null;
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
            if (!_isRun) return null;
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
        private bool _isRun = false;
    }
}
