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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class PortalDBEntities : DbContext
    {
        public PortalDBEntities()
            : base("name=PortalDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<EmployeeProject> EmployeeProjects { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<Note> Notes { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<WeeklyReport> WeeklyReports { get; set; }
        public virtual DbSet<MTS_PROJECTTYPE> MTS_PROJECTTYPE { get; set; }
        public virtual DbSet<MTS_EMAILMASTER> MTS_EMAILMASTER { get; set; }
        public virtual DbSet<MTS_EMAILSCHEDULE> MTS_EMAILSCHEDULE { get; set; }
        public virtual DbSet<MTS_EMAILTEMPLATE> MTS_EMAILTEMPLATE { get; set; }
        public virtual DbSet<MTS_GROUPS> MTS_GROUPS { get; set; }
        public virtual DbSet<MTS_SMTPDETAILS> MTS_SMTPDETAILS { get; set; }
        public virtual DbSet<MTS_SERVICECONFIG> MTS_SERVICECONFIG { get; set; }
        public virtual DbSet<WorkCode> WorkCodes { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<WorkCodesActivity> WorkCodesActivities { get; set; }
        public virtual DbSet<MTS_WORK_GROUP_MAPPING> MTS_WORK_GROUP_MAPPING { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Login> Logins { get; set; }
    }
}
