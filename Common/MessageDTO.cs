using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class MessageDTO
    {
        public String SenderLogin { get; set; }
        public String Message { get; set; }
        public String ReceiverLogin { get; set; }
        //public DateTime reciptDateTime { get; set; }
    }
}
