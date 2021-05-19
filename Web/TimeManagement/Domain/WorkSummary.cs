using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManagement.Domain
{
    public class WorkSummary
    {
        public string Employee { get; set; }
        public string Company { get; set; }
        public string WorkCode { get; set; }
        public decimal BillRate { get; set; }
        public decimal Hours { get; set; }
        public decimal Total { get; set; }
        public decimal Costperhour { get; set; }
        public decimal TotalCost { get; set; }
        public decimal PayableAmount { get; set; }
        public decimal BillableAmount { get; set; }
        public decimal Margin { get; set; }
    }
}