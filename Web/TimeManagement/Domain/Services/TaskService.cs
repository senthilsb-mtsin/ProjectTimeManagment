using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using TimeManagement.Models;
using TimeManagement.ViewModel;

namespace TimeManagement.Domain.Services
{
    public class TaskService : BaseService
    {
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public List<Task> GetTasks(string fromDate, string toDate, string userName, string location, string project, string workcodeActivity, string employee)
        {
            List<Task> tasksData = new List<Task>();
            List<Task> tasks = null;
            List<Task> interResult = new List<Task>();


            DateTime from = DateTime.Now.Date;
            DateTime to = DateTime.Now;


            if (!string.IsNullOrEmpty(fromDate))
                from = Convert.ToDateTime(fromDate);
            if (!string.IsNullOrEmpty(toDate))
                to = Convert.ToDateTime(toDate);

            tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCodesActivity").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to).OrderBy(x => x.ExecutionDate).ToList();

            if (!string.IsNullOrEmpty(userName))
            {
                interResult = new List<Task>();
                foreach (string val in userName.Split(','))
                {
                    interResult.AddRange(tasks.Where(x => x.Employee.Logins.FirstOrDefault().UserId == val).ToList());
                }
                tasks = interResult;
            }

            if (!string.IsNullOrEmpty(location))
            {

                interResult = new List<Task>();
                foreach (string val in location.Split(','))
                {
                    interResult.AddRange(tasks.Where(x => x.Employee.Location.Name == val).ToList());
                }
                tasks = interResult;
            }

            if (tasks != null)
            {
                if (!string.IsNullOrEmpty(project))
                {
                    interResult = new List<Task>();
                    foreach (string val in project.Split(','))
                    {
                        interResult.AddRange(tasks.Where(x => x.Project.Name == val).ToList());
                    }
                    tasks = interResult;
                }
            }

            if (tasks != null)
            {
                if (!string.IsNullOrEmpty(workcodeActivity))
                {
                    interResult = new List<Task>();
                    foreach (string val in workcodeActivity.Split(','))
                    {
                        interResult.AddRange(tasks.Where(x => x.WorkCodesActivity.Name == val).ToList());
                    }
                    tasks = interResult;
                }
            }

            if (tasks != null)
            {
                if (!string.IsNullOrEmpty(employee))
                {
                    tasks = tasks.Where(x => x.Employee.LastName + "," + x.Employee.FirstName == employee).ToList();
                }
            }

            tasksData = tasks;
            return tasksData;
        }
        public List<Employee> GetUnCompleteTasks(string fromDate, string toDate, string location)
        {
            List<Task> tasksData = new List<Task>();
            List<Task> tasks = null;
            // List<EmployeeRole> interResult = new List<EmployeeRole>();
            List<Employee> interResult = new List<Employee>();
            // List<EmployeeRole> Temp = null;
            List<Employee> Temp = new List<Employee>();
            DateTime from = DateTime.Now.Date;
            DateTime tempFromDate;
            DateTime to = DateTime.Now;


            if (!string.IsNullOrEmpty(fromDate))
                from = Convert.ToDateTime(fromDate);
            if (!string.IsNullOrEmpty(toDate))
                to = Convert.ToDateTime(toDate);


            DayOfWeek day = from.DayOfWeek;
            for (int i = 0; from <= to; i++)
            {
                if ((day >= DayOfWeek.Monday) && (day <= DayOfWeek.Friday))
                {
                    tempFromDate = from.AddHours(23);
                    tempFromDate = tempFromDate.AddMinutes(59);
                    tempFromDate = tempFromDate.AddSeconds(59);
                    tasks = this.db.Tasks.Include("Employee").Where(xp => xp.ExecutionDate >= from && xp.ExecutionDate <= tempFromDate).OrderBy(xp => xp.ExecutionDate).ToList();
                    var tt = tasks.Select(t => t.EmployeeId).ToList();
                    //var allemp = this.db.EmployeeRoles.Where(t => !tt.Contains(t.EmployeeId)).Select(t => t.EmployeeId).ToList();
                    var allemp = this.db.Employees.Where(t => !tt.Contains(t.Id)).Select(t => t.Id).ToList();
                    // List<EmployeeRole> uncompT = this.db.EmployeeRoles.Include("Employee").Where(x => allemp.Contains(x.EmployeeId)).ToList();
                    //List<EmployeeRole> uncompT = this.db.Employees.Where(x => allemp.Contains(x.Id)).ToList();
                    List<Employee> uncompT = this.db.Employees.Where(x => allemp.Contains(x.Id)).ToList();
                    if (Temp == null)
                    {
                        Temp = uncompT;
                    }
                    else
                    {
                        Temp = Temp.Union(uncompT).ToList();
                    }

                }
                from = from.AddDays(1);
                day = from.DayOfWeek;

            }



            if (!string.IsNullOrEmpty(location))
            {

                interResult = new List<Employee>();
                foreach (string val in location.Split(','))
                {
                    interResult.AddRange(Temp.Where(x => x.Location.Name == val).ToList());
                }
                Temp = interResult;
            }
            return Temp;
        }
        public List<Task> GetActivity(string fromDate, string toDate, string project, string location)
        {
            List<Task> tasksData = new List<Task>();
            List<Task> tasks = null;
            List<Task> interResult = new List<Task>();

            DateTime from = DateTime.Now.Date;
            DateTime to = DateTime.Now;


            if (!string.IsNullOrEmpty(fromDate))
                from = Convert.ToDateTime(fromDate);
            if (!string.IsNullOrEmpty(toDate))
                to = Convert.ToDateTime(toDate);
            tasks = this.db.Tasks.Include("Project").Include("Employee").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to).OrderBy(x => x.ExecutionDate).ToList();
            //  tasks = (this.db.Tasks.Include("Project").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to).GroupBy(x => x.ExecutionDate)).ToList();

            if (!string.IsNullOrEmpty(location))
            {

                interResult = new List<Task>();
                foreach (string val in location.Split(','))
                {
                    interResult.AddRange(tasks.Where(x => x.Employee.Location.Name == val).ToList());
                }
                tasks = interResult;
            }



            if (tasks != null)
            {
                if (!string.IsNullOrEmpty(project))
                {
                    interResult = new List<Task>();
                    foreach (string val in project.Split(','))
                    {
                        interResult.AddRange(tasks.Where(x => x.Project.Name == val).ToList());
                    }
                    tasks = interResult;
                }
            }

            var tsk = (from tk in tasks
                       group tk by new { tk.Employee, tk.Project, tk.ExecutionDate }
                           into tkGroup
                       select new Task() { Project = tkGroup.Key.Project, Employee = tkGroup.Key.Employee, ExecutionDate = tkGroup.Key.ExecutionDate, Hours = tkGroup.Sum(x => x.Hours) }).ToList();



            tasksData = tsk;
            return tasksData;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="wtData"></param>
        /// <returns></returns>
        public void GetWeekTasks(string fromDate, WeekTasks wtData, string project)
        {

            List<Task> tasks = null;
            if (!string.IsNullOrEmpty(fromDate))
            {
                DateTime fromdt = Convert.ToDateTime(fromDate);
                DateTime todt = Convert.ToDateTime(fromDate).AddDays(6);

                tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= fromdt && x.ExecutionDate <= todt).OrderBy(x => x.ExecutionDate).ToList();

                if (tasks != null)
                {
                    if (!string.IsNullOrEmpty(project))
                        tasks = tasks.Where(x => x.Project.Name == project).ToList();
                }

                var wt = from f in tasks
                         group f by new { f.EmployeeId }

                             into myGroup
                         select new
                         {
                             myGroup.Key.EmployeeId,
                             EmployeeFirstName = myGroup.Select(h => h.Employee.FirstName).FirstOrDefault(),
                             EmployeeLastName = myGroup.Select(h => h.Employee.LastName).FirstOrDefault(),
                             Date1 = myGroup.Where(f => f.ExecutionDate == fromdt).Sum(c => c.Hours),
                             Date2 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(1)).Sum(c => c.Hours),
                             Date3 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(2)).Sum(c => c.Hours),
                             Date4 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(3)).Sum(c => c.Hours),
                             Date5 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(4)).Sum(c => c.Hours),
                             Date6 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(5)).Sum(c => c.Hours),
                             Date7 = myGroup.Where(f => f.ExecutionDate == fromdt.AddDays(6)).Sum(c => c.Hours),
                             TotalHours = myGroup.Sum(c => c.Hours)
                         };


                wtData.StartDate = fromdt;
                wtData.EndDate = todt;
                if (wt != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("EmployeeId");
                    dt.Columns.Add("FirstName");
                    dt.Columns.Add("LastName");
                    dt.Columns.Add(fromdt.ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(1).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(2).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(3).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(4).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(5).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add(fromdt.AddDays(6).ToString("MM/dd/yyyy"), typeof(decimal));
                    dt.Columns.Add("Total Hours", typeof(decimal));
                    DataRow row = null;
                    foreach (var rowObj in wt)
                    {
                        row = dt.NewRow();
                        dt.Rows.Add(rowObj.EmployeeId
                            , rowObj.EmployeeFirstName
                            , rowObj.EmployeeLastName
                            , rowObj.Date1
                            , rowObj.Date2
                            , rowObj.Date3
                            , rowObj.Date4
                            , rowObj.Date5
                            , rowObj.Date6
                            , rowObj.Date7
                            , rowObj.TotalHours);
                    }
                    wtData.ReportData = dt;
                }

            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="userName"></param>
        /// <param name="location"></param>
        public BillingModel GetBilling(string fromDate, string toDate, string userName, string location, string Company = "")
        {
            BillingModel billingModel = new BillingModel();

            List<Task> tasks = null;

            if (!string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                DateTime from = Convert.ToDateTime(fromDate);
                if (!string.IsNullOrEmpty(Company))
                {
                    tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.Employee.CompanyName.Equals(Company)).OrderBy(x => x.ExecutionDate).ToList();
                }
                else if (userName == null)
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate == from).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Location.Name.Equals(location)).ToList();
                }
                else
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("Login").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName)).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("Login").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName) && x.Employee.Location.Name.Equals(location)).ToList();
                }
            }
            else
            {
                DateTime from = Convert.ToDateTime(fromDate);
                DateTime to = Convert.ToDateTime(toDate);

                if (!string.IsNullOrEmpty(Company))
                {
                    tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.CompanyName.Equals(Company)).OrderBy(x => x.ExecutionDate).ToList();
                }

                else if (userName == null)
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to).OrderBy(x => x.ExecutionDate).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Location.Name.Equals(location)).OrderBy(x => x.ExecutionDate).ToList();

                }
                else
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName)).OrderBy(x => x.ExecutionDate).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName) && x.Employee.Location.Name.Equals(location)).OrderBy(x => x.ExecutionDate).ToList();
                }
            }

            tasks.ForEach(x =>
            {
                WorkSummary workSummary = billingModel.Summary.Where(y => y.WorkCode.Equals(x.WorkCodesActivity.Name) && y.Employee.Equals(x.Employee.LastName + ", " + x.Employee.FirstName)).FirstOrDefault();

                if (workSummary == null)
                {
                    workSummary = new WorkSummary();
                    workSummary.WorkCode = x.WorkCodesActivity.Name;
                    workSummary.Employee = x.Employee.LastName + ", " + x.Employee.FirstName;
                    workSummary.BillRate = (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value : 0);
                    if (!string.IsNullOrEmpty(Company))
                    {
                        workSummary.PayableAmount = (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                        workSummary.Costperhour = (x.Employee.Cost.HasValue ? x.Employee.Cost.Value : 0);
                    }
                    workSummary.BillableAmount = (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value * x.Hours.Value : 0);
                    workSummary.Company = x.Employee.CompanyName;
                    workSummary.Hours = x.Hours.Value;
                    workSummary.Total = x.Charge.Value;

                    billingModel.Summary.Add(workSummary);
                }
                else
                {
                    if (!string.IsNullOrEmpty(Company))
                    {
                        workSummary.PayableAmount += (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                    }
                    else
                    {
                        workSummary.BillableAmount += (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value * x.Hours.Value : 0);
                    }
                    workSummary.Hours += x.Hours.Value;
                    workSummary.Total += x.Charge.Value;
                }

            });

            billingModel.Summary = billingModel.Summary.OrderBy(x => x.Company).OrderBy(x => x.Employee).ToList();

            return billingModel;
        }


        public BillingModel GetMTSBilling(string fromDate, string toDate, string userName, string location)
        {
            BillingModel billingModel = new BillingModel();

            List<Task> tasks = null;

            if (!string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                DateTime from = Convert.ToDateTime(fromDate);

                if (userName == null)
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate == from).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Location.Name.Equals(location)).ToList();
                }
                else
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("Login").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName)).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("Login").Include("WorkCode").Where(x => x.ExecutionDate == from && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName) && x.Employee.Location.Name.Equals(location)).ToList();
                }
            }
            else
            {
                DateTime from = Convert.ToDateTime(fromDate);
                DateTime to = Convert.ToDateTime(toDate);

                if (userName == null)
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to).OrderBy(x => x.ExecutionDate).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Location.Name.Equals(location)).OrderBy(x => x.ExecutionDate).ToList();
                }
                else
                {
                    if (location == null)
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName)).OrderBy(x => x.ExecutionDate).ToList();
                    else
                        tasks = this.db.Tasks.Include("Project").Include("Employee").Include("WorkCode").Where(x => x.ExecutionDate >= from && x.ExecutionDate <= to && x.Employee.Logins.FirstOrDefault().UserId.Equals(userName) && x.Employee.Location.Name.Equals(location)).OrderBy(x => x.ExecutionDate).ToList();
                }
            }

            tasks.ForEach(x =>
            {
                WorkSummary workSummary = billingModel.Summary.Where(y => y.WorkCode.Equals(x.WorkCodesActivity.Name) && y.Employee.Equals(x.Employee.LastName + ", " + x.Employee.FirstName)).FirstOrDefault();

                if (workSummary == null)
                {
                    workSummary = new WorkSummary();
                    workSummary.WorkCode = x.WorkCodesActivity.Name;
                    workSummary.Employee = x.Employee.LastName + ", " + x.Employee.FirstName;
                    workSummary.BillRate = (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value : 0);
                    workSummary.Company = x.Employee.CompanyName;
                    workSummary.Hours = x.Hours.Value;
                    workSummary.Total = x.Charge.Value;
                    workSummary.Costperhour = (x.Employee.Cost.HasValue ? x.Employee.Cost.Value : 0);
                    workSummary.TotalCost = (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                    workSummary.BillableAmount = (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value * x.Hours.Value : 0);
                    workSummary.PayableAmount = (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                    workSummary.Margin = workSummary.BillableAmount - workSummary.PayableAmount;
                    //(x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value * x.Hours : 0) - (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours : 0);

                    billingModel.Summary.Add(workSummary);
                }
                else
                {
                    //decimal MarginSub;
                    //MarginSub = workSummary.BillableAmount - workSummary.PayableAmount;
                    //workSummary.Margin += MarginSub;
                    //-workSummary.BillableAmount - workSummary.PayableAmount;
                    workSummary.TotalCost += (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                    workSummary.PayableAmount += (x.Employee.Cost.HasValue ? x.Employee.Cost.Value * x.Hours.Value : 0);
                    workSummary.BillableAmount += (x.Employee.BillRate.HasValue ? x.Employee.BillRate.Value * x.Hours.Value : 0);
                    workSummary.Margin = workSummary.BillableAmount - workSummary.PayableAmount;
                    workSummary.Hours += x.Hours.Value;
                    workSummary.Total += x.Charge.Value;

                }

            });

            billingModel.Summary = billingModel.Summary.OrderBy(x => x.Company).OrderBy(x => x.Employee).ToList();

            return billingModel;
        }

        #endregion

        #region Private Methods

        #endregion
        public DataTable getcategorydata(DateTime from, DateTime to, Int16 LocationId)
        {


            DataTable dt = new DataTable();

            var conn = db.Database.Connection;
            var connectionState = conn.State;
            try
            {
                using (db)
                {
                    if (connectionState != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "TREX_CATEGORY_REPORT";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("FROMDATE", from));
                        cmd.Parameters.Add(new SqlParameter("TODATE", to));
                        cmd.Parameters.Add(new SqlParameter("LOCATIONID", LocationId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                    conn.Close();
            }

            return dt;

        }

        public List<SelectListItem> getlocations()
        {
            List<SelectListItem> location = new List<SelectListItem>();

            location = (from loc in this.db.Locations.AsEnumerable().OrderBy(x => x.Name)
                        select new SelectListItem
                        {
                            Value = loc.Id.ToString(),
                            Text = loc.Name.ToString()
                        }).ToList();
            return location;
        }

        public DataTable GetProjectWise(string fromDate, string toDate, Int32 location)
        {
            DataTable dt = new DataTable();
            DateTime from = Convert.ToDateTime(fromDate);
            DateTime to = Convert.ToDateTime(toDate);

            var conn = db.Database.Connection;
            var connectionState = conn.State;
            try
            {
                using (db)
                {
                    if (connectionState != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "TREX_PROJECT_REPORT";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("FROMDATE", from));
                        cmd.Parameters.Add(new SqlParameter("TODATE", to));
                        cmd.Parameters.Add(new SqlParameter("LOCATIONID", location));
                        using (IDataReader reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                    conn.Close();
            }
            return dt;
        }
        public DataTable getActivityData(string from, string to, Int16 ProjectId, Int16 LocationId)
        {

            DataTable dt = new DataTable();

            var conn = db.Database.Connection;
            var connectionState = conn.State;
            try
            {
                using (db)
                {
                    if (connectionState != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "EMPLOYEE_ACTIVITY_WISE_SUMMARY_REPORT";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("FROMDATE", from));
                        cmd.Parameters.Add(new SqlParameter("TODATE", to));
                        cmd.Parameters.Add(new SqlParameter("PROJECT_ID", ProjectId));
                        cmd.Parameters.Add(new SqlParameter("LOCATION_ID", LocationId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                    conn.Close();
            }

            return dt;
        }
        public List<SelectListItem> ProjectList()
        {

            List<SelectListItem> projects = new List<SelectListItem>();
            projects = (from project in this.db.Projects.AsEnumerable().OrderBy(x => x.Name)
                        select new SelectListItem
                        {
                            Value = project.Id.ToString(),
                            Text = project.Name.ToString()
                        }).ToList();
            return projects;
        }

        public DataTable getEmployeeDetailedActivity(string from, string to, Int16 ProjectId, Int16 EmployeeId)
        {

            DataTable dt = new DataTable();

            var conn = db.Database.Connection;
            var connectionState = conn.State;
            try
            {
                using (db)
                {
                    if (connectionState != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "EMPLOYEE_DATEWISE_SUMMARY_REPORT";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("FROMDATE", from));
                        cmd.Parameters.Add(new SqlParameter("TODATE", to));
                        cmd.Parameters.Add(new SqlParameter("PROJECT_ID", ProjectId));
                        cmd.Parameters.Add(new SqlParameter("RESOURCE_ID", EmployeeId));
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                    conn.Close();
            }

            return dt;
        }

        public DataTable getEmployee(Int16 ProjectId)
        {

            DataTable dt = new DataTable();
            var conn = db.Database.Connection;
            var connectionState = conn.State;
            try
            {
                using (db)
                {
                    if (connectionState != ConnectionState.Open)
                        conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "GET_EMPLOYEE_BASED_ON_PROJECTID";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("PROJECT_ID", ProjectId));

                        using (var reader = cmd.ExecuteReader())
                        {
                            dt.Load(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (connectionState != ConnectionState.Open)
                    conn.Close();
            }

            return dt;
        }

        public List<SelectListItem> EmployeeList()
        {

            List<SelectListItem> projects = new List<SelectListItem>();
            projects = (from project in this.db.Employees.AsEnumerable().OrderBy(x => x.FirstName)
                        select new SelectListItem
                        {
                            Value = project.Id.ToString(),
                            Text = project.FirstName.ToString()
                        }).ToList();
            return projects;
        }




    }



}
