using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TimeManagement.ViewModel
{
    public class WorkCodeModel

    {
       
        public string DT_RowId { get; set; }
        public int id { get; set; }
        
        
        public string Name { get; set; }
        public string Number { get; set; }
        public string Billable { get; set; }
                
    }

 
}