using Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataModels
{
    public class User
    {
        [PrimaryKey, MaxLength(32)]
        public string Login { get; set; }
        [MaxLength(128), NotNull]
        public string Password { get; set; }
        [NotNull]
        public PresenceStatus Status { get; set; }
    }
}
