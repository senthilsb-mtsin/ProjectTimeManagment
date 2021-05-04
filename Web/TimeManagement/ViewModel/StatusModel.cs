using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TimeManagement.ViewModel
{
    public class StatusModel
    {
        public int taskId { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string projectId { get; set; }
        public string workCodeId { get; set; }
        public string Project { get; set; }
        public string workCode { get; set; }
        public int Scope { get; set; }
        public int Schedule { get; set; }
        public int Quality { get; set; }
        public int ClientSatisfaction { get; set; }
        public string ProjectStatus { get; set; }
        public string Risk { get; set; }            
    }
}