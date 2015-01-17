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
        Thread th;

        public Listening()
        {
            var builder = new ContainerBuilder();
            builder.Register(_ => new ConnectionFactory { HostName = Const.HostName }).As<IConnectionFactory>();
            builder.RegisterType<Modules.PresenceStatus>().As<IPresenceStatus>();
            builder.RegisterType<Modules.Messages>().As<IMessages>();
            builder.RegisterType<Modules.Activity>().As<IActivity>();
            _container = builder.Build();

            _isRun = true;

            ActivityQueue = new Queue<ActivityResponse>();
            PresenceQueue = new Queue<PresenceStatusNotification>();
            MessageQueue = new Queue<MessageResponse>();

            Thread th = new Thread(doExecute);
            th.Start();
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

        void doExecute() 
        {
            while (_isRun)
            {
                var msg = ListeningMessages();
                if (msg != null)
                    MessageQueue.Enqueue(msg);

                var act = ListeningActivity();
                if (act != null)
                    ActivityQueue.Enqueue(act);

                var prs = ListeningPresenceStatus();
                if (prs != null)
                    PresenceQueue.Enqueue(prs);

                Thread.Sleep(Timeout);
            }
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


        public Queue<ActivityResponse> ActivityQueue { get; set; }
        public Queue<MessageResponse> MessageQueue { get; set; }
        public Queue<PresenceStatusNotification> PresenceQueue { get; set; }

        private IContainer _container;
        private const int Timeout = 200;
    }
}
