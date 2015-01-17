using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Server.DataModels;
using Server.Modules;
using Topshelf;
using SQLite;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //example
            Console.WriteLine(" [x] Awaiting RPC requests");
            ServiceFactory.CreateService().Run();
        }
    }
}
