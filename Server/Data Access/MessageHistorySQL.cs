using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;

namespace Server.Data_Access
{
    //class for SQL connection
    public class MessageHistorySQL : IMessageHistoryDataAccess, IDisposable
    {
        public void SaveMessage(MessageReq message)
        {
            throw new NotImplementedException();
        }
        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                // Free other state (managed objects).
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.
            _disposed = true;
        }

        // Use C# destructor syntax for finalization code.
        ~MessageHistorySQL()
        {
            // Simply call Dispose(false).
            Dispose (false);
        }

        private bool _disposed = false;
    }
}
