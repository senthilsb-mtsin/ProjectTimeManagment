//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TimeManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class WeeklyReport
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WeeklyReport()
        {
            this.Notes = new HashSet<Note>();
        }
    
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        public int Scope { get; set; }
        public int Schedule { get; set; }
        public int Quality { get; set; }
        public int ClientSatisfaction { get; set; }
        public string ProjectStatus { get; set; }
        public string Risk { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime From { get; set; }
        public System.DateTime To { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Note> Notes { get; set; }
        public virtual Project Project { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
