using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace Server.Modules
{
    class ServiceFactory
    {
        public static Host createRegistrationService()
        {
            var host = HostFactory.New(x =>
            {
                x.Service<RegistrationService>(s =>
                {
                    s.ConstructUsing(name => new RegistrationService());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });

                x.RunAsLocalService();
                x.SetDescription("Service for handling registrations");
                x.SetDisplayName("RegistrationService");
                x.SetServiceName("RegistrationService");
            });
            return host;
        }
    }
}
