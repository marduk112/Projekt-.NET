﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Server.Modules
{
    class ServiceFactory
    {
        public static Host CreateRegistrationService()
        {
            return CreateService<RegistrationService>("RegistrationService",
                "Service that handles user registration");
        }

        public static Host CreateLoginService()
        {
            return CreateService<LoginService>("LoginService",
                "Service for handling user logon");
        }

        private static Host CreateService<T>(string serviceName, string serviceDescription) 
            where T : class, IServicable, new()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<T>(s =>
                {
                    s.ConstructUsing(name => new T());
                    s.WhenStopped(tc => tc.Stop());
                    s.WhenStarted(tc => tc.Start());
                });
                x.StartManually();
                x.RunAsLocalService();
                x.SetDescription(serviceDescription);
                x.SetDisplayName(serviceName);
                x.SetServiceName(serviceName);
            });
            return host;
        }

        public static ICollection<Func<Host>> ReturnServices()
        {
            List<Func<Host>> servicesList = new List<Func<Host>>();
            servicesList.Add(CreateRegistrationService);
            servicesList.Add(CreateLoginService);
            return servicesList;
        }
    }
}