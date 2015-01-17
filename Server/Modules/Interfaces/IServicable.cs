using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Server.Modules
{
    interface IServicable
    {
        void Start();
        void Stop();
    }
}
