using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TimeManagement.ViewModel
{
    public class ConfigurationModel

    {
       
        public string DT_RowId { get; set; }
        public int id { get; set; }
        
        
        public string Name { get; set; }
        public string Percentage { get; set; }
        public string Rate { get; set; }
                
    }

 
}