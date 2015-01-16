﻿using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DataModels
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(32), NotNull, Unique]
        public string Login { get; set; }
        [MaxLength(32), NotNull]
        public string Password { get; set; }
    }
}