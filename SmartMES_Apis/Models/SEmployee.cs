﻿using System;
using System.Collections.Generic;

namespace SmartMES_Apis.Models
{
    public partial class SEmployee
    {
        public string EmpCode { get; set; }
        public string EmpName { get; set; }
        public string PassWprd { get; set; }
        public string PhoneNum { get; set; }
        public string Address { get; set; }
        public int? Sex { get; set; }
        public string DepartCode { get; set; }
        public int IsStaff { get; set; }
        public string EntryDate { get; set; }
        public int? Enable { get; set; }
        public string Description { get; set; }
    }
}
