﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Modules
{
    class Service : IServicable
    {
        ICollection<IServicable> services = new List<IServicable>();
        ICollection<Thread> threads = new List<Thread>(); 

        public void Start()
        {
            
            services.Add(new RegistrationService());
            services.Add(new LoginService());
            services.Add(new UsersListService());

            foreach (var service in services)
            {
                threads.Add(new Thread(() => service.Start()));
            }

            RunAll();
        }

        public void RunAll()
        {
            foreach (Thread thread in threads)
            {
                thread.Start();
            }
        }

        public void Stop()
        {
            foreach (var service in services)
            {
                service.Stop();
            }
        }
    }
}
