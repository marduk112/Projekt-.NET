using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Modules;
using Topshelf;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //example
            Console.WriteLine(" [x] Awaiting RPC requests");
            var services = ServiceFactory.ReturnServices();
            foreach (var service in services)
            {
                new Thread(() =>
                {
                    service().Run();
                }).Start();
                Thread.Sleep(500); //It doesn't work without it
            }

            Console.ReadKey();
        }
    }
}
