using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace Client.Interfaces
{
    public interface IActivity : IDisposable
    {
        ActivityResponse ActivityResponse(int timeout);
        void ActivityReq(ActivityReq activityReq);
    }
}
