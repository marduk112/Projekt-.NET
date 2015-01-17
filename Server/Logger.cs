using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server
{
    class Logger
    {
        public static void serviceLog(Response response, Request request, string logMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now.ToLongTimeString());
            sb.Append(" - ");
            sb.Append(request.Login);
            sb.Append(logMsg);
            sb.Append(response.Status);
            Console.WriteLine(sb.ToString());
        }
    }
}
