﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace Server.Modules
{
    interface INotificationSender
    {
        void Send(Notification notification);
    }
}
