﻿using System;
using System.Collections.Generic;

namespace SmartMES_Apis.Models
{
    public partial class PFailDetailNgReasonCollect
    {
        public int Id { get; set; }
        public string WorkshopCode { get; set; }
        public string ProductCode { get; set; }
        public string ReasonCode { get; set; }
        public string ReasontypeCode { get; set; }
        public string Years { get; set; }
        public string Months { get; set; }
        public string Days { get; set; }
        public string Weeks { get; set; }
        public decimal? TotalQty { get; set; }
        public string Remarks { get; set; }
    }
}
