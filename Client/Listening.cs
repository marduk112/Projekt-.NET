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
    public class Listening : IDisposable
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
                user = response.ReceiveUserPresenceStatus(Timeout);
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
                messageResponse = response.ReceiveMessage(Timeout);
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
                activityResponse = response.ActivityResponse(Timeout);
                return activityResponse;
            }
            catch { }
            return null;
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Free other state (managed objects).
                _container.Dispose();
                _container = null;
            }
            scope.Dispose();
            scope = null;
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            _disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        ~Listening()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }

        private IContainer _container;
        private const int Timeout = 200;
        private bool _disposed = false;
        private ILifetimeScope scope;
    }
}
