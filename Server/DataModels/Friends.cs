using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataModels
{
    public class Friends
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string UserLogin1 { get; set; }
        [Indexed]
        public string UserLogin2 { get; set; }
    }
}
