using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TimeManagement.ViewModel
{
    public class EmployeeModel

    {
       
        public int DT_RowId { get; set; }
        public int Employeeid { get; set; }
        public string EmployeeCode { get; set; }
        
        
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public Nullable<bool> IsCommon { get; set; }
        public string BillRate { get; set; }
        public string CompanyName { get; set; }
        public string LocationId { get; set; }
        public string Cost { get; set; }
        public string DOB { get; set; }
        public string DOJ { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        
                
    }

    
}