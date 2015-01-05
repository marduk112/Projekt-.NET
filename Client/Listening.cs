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
    public static class Listening
    {
        public static void Start()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            builder.RegisterType<Modules.Messages>().As<IMessages>();
            builder.RegisterType<Modules.Activity>().As<IActivity>();
            _container = builder.Build();
            _isRun = true;
        }

        public static User ListeningPresenceStatus()
        {
            if (!_isRun) return null;
            User user;
            using (var scope = _container.BeginLifetimeScope())
            {
                var response = scope.Resolve<IPresenceStatus>();
                user = response.ReceiveUserPresenceStatus(Timeout);
            }
            return user;
        }

        public static MessageResponse ListeningMessages()
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

        public static ActivityResponse ListeningActivity()
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

        private static IContainer _container;
        private const int Timeout = 200;
        private static bool _isRun = false;
    }
}
