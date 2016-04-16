﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AS2_Companion
{
    public class Scoreboard
    {
        public Category Global;
        public Category Friends;
        public Category Regional;

        public class Category
        {
            public List<Entry> Entries = new List<Entry>();
        }

        public class Entry
        {
            public string UserID { get; set; }
            public string SteamID { get; set; }
            public string Score { get; set; }
            public string RideTime { get; set; }
            public string Mode { get; set; }
            //public string Comment { get; set; }
            public string Username { get; set; }
        }
    }
}
