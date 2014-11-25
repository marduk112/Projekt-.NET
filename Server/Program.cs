using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server.Modules;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //example
            Console.WriteLine(" [x] Awaiting RPC requests");
            new Thread(Login.login).Start();
            new Thread(Registration.registration).Start();
            Console.ReadKey();
        }
    }
}
