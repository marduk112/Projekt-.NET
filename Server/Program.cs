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
            Thread registerThread = new Thread(Registration.registration) {IsBackground = true};
            registerThread.Start();

            var readLine = Console.ReadLine();
            while (readLine != null && !readLine.Equals("koniec"))
            {
                
            }
        }
    }
}
