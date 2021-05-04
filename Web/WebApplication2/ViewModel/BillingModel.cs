using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeManagement.Domain;
using TimeManagement.Models;

namespace TimeManagement.ViewModel
{
    public class BillingModel
    {
        //Key: employee id
        public List<WorkSummary> Summary { get; set; }

        public BillingModel()
        {
            this.Summary = new List<WorkSummary>();
        }
    }
}