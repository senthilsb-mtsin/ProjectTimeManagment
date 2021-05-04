using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManagement.ViewModel
{
    public class TaskModel
    {
        public int taskId { get; set; }
        public string executionDate { get; set; }
        public string projectId { get; set; }
        public string project { get; set; }
        public string workCodeId { get; set; }
        public string workCode { get; set; }
        public decimal hours { get; set; }
        public string description { get; set; }
        //public string removeTask { get; set; }
    }
}