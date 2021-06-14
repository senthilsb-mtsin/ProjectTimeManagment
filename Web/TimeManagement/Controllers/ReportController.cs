using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using TimeManagement.Domain;
using TimeManagement.Models;
using TimeManagement.ViewModel;

namespace TimeManagement.Controllers
{
    public class ReportController : BaseController
    {
        #region Private Methdos

        private List<SelectListItem> GetLocations()
        {
            List<string> locations = this.db.Locations.OrderBy(x => x.Name).Select(x => x.Name).ToList();

            List<SelectListItem> items = new List<SelectListItem>();
            // items.Add(new SelectListItem { Text = "", Value = "" });

            locations.ForEach(x =>
            {
                items.Add(new SelectListItem { Text = x, Value = x });
            });

            return items;
        }

        private List<SelectListItem> GetProjects()
        {
            List<string> Projects = this.db.Projects.OrderBy(x => x.Name).Select(x => x.Name).ToList();

            List<SelectListItem> items = new List<SelectListItem>();
            //    items.Add(new SelectListItem { Text = "", Value = "" });

            Projects.ForEach(x =>
            {
                items.Add(new SelectListItem { Text = x, Value = x });
            });

            return items;
        }

       
        private Dictionary<int, string> GetWorkCodes()
        {
            List<int> Id = this.db.WorkCodes.OrderBy(x => x.Id).Select(x => x.Id).ToList();
            List<string> WorkCodes = this.db.WorkCodes.OrderBy(x => x.Name).Select(x => x.Name).ToList();


            Dictionary<int, string> items = new Dictionary<int, string>(Id.Count);
            for (int i = 0; i < Id.Count; i++)
            {
                items.Add(Id[i], WorkCodes[i]);
            }

            return items;
        }
        private List<SelectListItem> GetWorkCodeActivities()
        {



            return new List<SelectListItem>();
        }
        public JsonResult GetWorkCodeActivities(int? id)
        {
            List<Select2ListModel> items = new List<Select2ListModel>();
            if (id != null)
            {
                List<string> WorkCodeActivities = this.db.WorkCodesActivities.Where(x => x.WorkCodeId == id).OrderBy(x => x.Name).Select(x => x.Name).ToList();
                items = this.db.WorkCodesActivities.Where(x => x.WorkCodeId == id).OrderBy(x => x.Name).Select(x => new Select2ListModel() { id = x.Name, text = x.Name }).ToList();

            }
            return Json(items, JsonRequestBehavior.AllowGet);
        }

        private List<SelectListItem> GetEmployees()
        {
            List<string> Employees = this.db.Employees.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).Select(x => x.LastName + "," + x.FirstName).ToList();

            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Select Employee", Value = "" });

            Employees.ForEach(x =>
            {
                items.Add(new SelectListItem { Text = x, Value = x });
            });

            return items;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult TimeByDate()
        {
            ViewBag.locations = this.GetLocations();
            ViewBag.Projects = this.GetProjects();
            ViewBag.WorkCodes = this.GetWorkCodes();
            ViewBag.WorkCodeActivity = this.GetWorkCodeActivities();
            ViewBag.Employees = this.GetEmployees();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public ActionResult EmployeeActivities()
        {
            ViewBag.locations = this.GetLocations();

            ViewBag.Projects = this.GetProjects();

            return View();
        }

        public ActionResult TimeSheetManagement()
        {
            ViewBag.locations = this.GetLocations();

            return View();
        }

        public ActionResult GetUnCompleteTask(string fromDate, string toDate, string location)
        {
            List<Employee> tasks = null;

            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                // fromDate = DateTime.Now.ToShortDateString();

                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetUnCompleteTasks(fromDate, toDate, location);
                else
                    tasks = this.taskService.GetUnCompleteTasks(fromDate, toDate, location);
                ViewBag.Message = "Result of " + DateTime.Now.ToShortDateString();
            }
            else
            {
                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetUnCompleteTasks(fromDate, toDate, location);
                else
                    tasks = this.taskService.GetUnCompleteTasks(fromDate, toDate, location);

                if (!string.IsNullOrEmpty(toDate))
                    ViewBag.Message = "Result from " + fromDate + " to " + toDate;
                else
                    ViewBag.Message = "Result of " + fromDate;
            }
            if (tasks != null)
            {
                var result = from task in tasks
                             select new
                             {
                                 EmployeeName = task.LastName + ',' + task.FirstName
                             };

                Session["Tasks"] = tasks;

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult CategoryWiseReport()
        {
            ViewBag.Locations = taskService.getlocations();
            return View();
        }


        public ActionResult CategoryWiseReport(string fromdate, string todate, string LocationId)
        {
            ViewBag.resultcount = false;
            CategoryWiseModel objcat = new CategoryWiseModel();

            if (LocationId == null || LocationId == "")
            {
                LocationId = "0";
            }

            objcat.data = taskService.getcategorydata(Convert.ToDateTime(fromdate), Convert.ToDateTime(todate), Int16.Parse(LocationId));

            if (objcat.data.Rows.Count == 0)
            {
                ViewBag.resultcount = true;
            }
            return PartialView("~/Views/Report/_PVcategoryreport.cshtml", objcat);
        }

        public ActionResult GetEmployeeActivity(string fromDate, string toDate, string project, string location)
        {
            List<Task> tasks = null;
            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                // fromDate = DateTime.Now.ToShortDateString();

                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetActivity(fromDate, toDate, project, location);
                else
                    tasks = this.taskService.GetActivity(fromDate, toDate, project, location);
                ViewBag.Message = "Result of " + DateTime.Now.ToShortDateString();
            }
            else
            {
                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetActivity(fromDate, toDate, project, location);
                else
                    tasks = this.taskService.GetActivity(fromDate, toDate, project, location);

                if (!string.IsNullOrEmpty(toDate))
                    ViewBag.Message = "Result from " + fromDate + " to " + toDate;
                else
                    ViewBag.Message = "Result of " + fromDate;
            }

            if (tasks != null)
            {
                var result = from task in tasks
                             select new
                             {
                                 Date = Convert.ToDateTime(task.ExecutionDate).ToString("MM/dd/yyyy"),
                                 EmployeeName = task.Employee.LastName + ',' + task.Employee.FirstName,
                                 ProjectName = task.Project.Name,
                                 Hours = task.Hours
                                 // Hours = Convert.ToDecimal(task.Hours).ToString("0.00")                        


                             };

                var rslt = from res in result
                           group res by new { res.Date, res.EmployeeName, res.ProjectName }
                               into grp
                           select new { grp.Key.Date, grp.Key.EmployeeName, grp.Key.ProjectName, Hours = Convert.ToDecimal(grp.Sum(x => (x.Hours))).ToString("0.00") };



                Session["Tasks"] = tasks;

                var resultData = new
                {
                    aaData = rslt
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public ActionResult GetTimeByDate(string fromDate, string toDate, string userName, string location, string project, string workcodeActivity, string employee)
        {
            if (string.IsNullOrEmpty(location))
                location = null;

            List<Task> tasks = null;
            Session["Tasks"] = null;

            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                // fromDate = DateTime.Now.ToShortDateString();

                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetTasks(fromDate, toDate, null, location, project, workcodeActivity, employee);
                else
                    tasks = this.taskService.GetTasks(fromDate, toDate, User.Identity.Name, location, project, workcodeActivity, employee);

                ViewBag.Message = "Result of " + DateTime.Now.ToShortDateString();
            }
            else
            {
                if (User.IsInRole(Constants.ROLE_ADMIN))
                    tasks = this.taskService.GetTasks(fromDate, toDate, null, location, project, workcodeActivity, employee);
                else
                    tasks = this.taskService.GetTasks(fromDate, toDate, User.Identity.Name, location, project, workcodeActivity, employee);

                if (!string.IsNullOrEmpty(toDate))
                    ViewBag.Message = "Result from " + fromDate + " to " + toDate;
                else
                    ViewBag.Message = "Result of " + fromDate;
            }


            var result = from task in tasks
                         select new
                         {
                             LocationName = task.Employee.Location.Name,
                             EmployeeName = task.Employee.LastName + ',' + task.Employee.FirstName,
                             ProjectName = task.Project.Name,
                             WorkCodeName = task.WorkCodesActivity.Name,
                             Hours = Convert.ToDecimal(task.Hours).ToString("0.00"),
                             WorkCode = this.db.WorkCodes.FirstOrDefault(x=>x.Id==task.WorkCodesActivity.WorkCodeId).Name,
                             Description = task.Description,
                             Date = Convert.ToDateTime(task.ExecutionDate).ToString("MM/dd/yyyy")

                         };

            Session["Tasks"] = tasks;

            var resultData = new
            {
                aaData = result
            };
            return Json(resultData, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult TimeByWeek()
        {
            ViewBag.Projects = this.GetProjects();
            return View(new WeekTasks());
        }
        [HttpPost]
        public ActionResult TimeByWeek(string dummy)
        {
            string project = Request.Form["Projects"];
            string fromDate = Request.Form["fromDate"];
            WeekTasks taskData = new WeekTasks();

            if (string.IsNullOrEmpty(fromDate))
            {
                fromDate = DateTime.Now.ToString("MM/dd/yyyy");
            }
            this.taskService.GetWeekTasks(fromDate, taskData, project);
            ViewBag.Message = "Result from " + fromDate + " to " + Convert.ToDateTime(taskData.EndDate).ToString("MM/dd/yyyy ");

            ViewBag.Projects = this.GetProjects();
            return View(taskData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FileResult ExportTaskDetails()
        {
            string output = null;

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine("Company,Consultant,Date,Project, Work Code, Hours, Description");

            List<Task> tasks = Session["Tasks"] as List<Task>;

            if (tasks != null)
            {
                tasks.ForEach(x =>
                {

                    output = string.Format("{0},\"{1}\",{2},{3},{4},{5},\"{6}\"", x.Employee.Location.Name, x.Employee.LastName + " " + x.Employee.FirstName, x.ExecutionDate.ToString("MM/dd/yyyy"), x.Project.Name, x.WorkCodesActivity.Name, x.Hours, x.Description);
                    streamWriter.WriteLine(output);
                });
            }

            streamWriter.Flush();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "Export.csv");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Billing()
        {
            ViewBag.locations = this.GetLocations();
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dummy"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Billing(string dummy)
        {
            string fromDate = Request.Form["fromDate"];
            string toDate = Request.Form["toDate"];
            string location = Request.Form["locations"];
            string Company = dummy == "MonthlyAccountPayable" ? Request.Form["Company"] : "";
            if (string.IsNullOrEmpty(location))
                location = null;

            BillingModel billingModel = null;
            Session["BillingModel"] = null;

            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                fromDate = DateTime.Now.ToShortDateString();

                if (User.IsInRole(Constants.ROLE_ADMIN))
                    billingModel = this.taskService.GetBilling(fromDate, toDate, null, location, Company);
                else
                    billingModel = this.taskService.GetBilling(fromDate, toDate, User.Identity.Name, location, Company);

                ViewBag.Message = "Result of " + DateTime.Now.ToShortDateString();
            }
            else
            {
                if (User.IsInRole(Constants.ROLE_ADMIN))
                    billingModel = this.taskService.GetBilling(fromDate, toDate, null, location, Company);
                else
                    billingModel = this.taskService.GetBilling(fromDate, toDate, User.Identity.Name, location, Company);

                if (!string.IsNullOrEmpty(toDate))
                    ViewBag.Message = "Result from " + fromDate + " to " + toDate;
                else
                    ViewBag.Message = "Result of " + fromDate;
            }

            Session["BillingModel"] = billingModel;
            if (dummy != "MonthlyAccountPayable")
                ViewBag.locations = this.GetLocations();
            if (dummy == "Monthly")
            {
                return View("MonthlyBilling", billingModel);
            }
            else if (dummy == "MonthlyAccountPayable")
            {
                return View("MonthlyAccountsPayable", billingModel);
            }
            else
            {
                return View(billingModel);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public FileResult ExportBillingDetails()
        {
            string output = null;

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine("Company,Consultant,Work Code, Hours");

            BillingModel billingModel = Session["BillingModel"] as BillingModel;

            if (billingModel != null)
            {
                billingModel.Summary.ForEach(x =>
                {
                    output = string.Format("{0},\"{1}\",{2},{3}", x.Company, x.Employee, x.WorkCode, x.Hours);
                    streamWriter.WriteLine(output);
                });
            }

            streamWriter.Flush();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "Export.csv");
        }


        [HttpPost]
        public ActionResult MonthlyBilling(string d)
        {
            string dummy = "Monthly";
            return Billing(dummy);
        }

        [Authorize]
        public ActionResult MonthlyBilling()
        {
            ViewBag.locations = this.GetLocations();
            return View();
        }

        [Authorize]
        public ActionResult MonthlyAccountsPayable()
        {

            return View();
        }

        [HttpPost]
        public ActionResult MonthlyAccountsPayable(string d)
        {
            string dummy = "MonthlyAccountPayable";
            return Billing(dummy);
        }

        [Authorize]
        public ActionResult MTSInternalbilling()
        {
            ViewBag.locations = this.GetLocations();
            return View();
        }
        [HttpPost]
        public ActionResult MTSInternalbilling(string d)
        {

            string fromDate = Request.Form["fromDate"];
            string toDate = Request.Form["toDate"];
            string location = Request.Form["locations"];

            if (string.IsNullOrEmpty(location))
                location = null;

            BillingModel billingModel = null;
            Session["BillingModel"] = null;

            if (string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate))
            {
                fromDate = DateTime.Now.ToShortDateString();

                if (User.IsInRole(Constants.ROLE_ADMIN))
                    billingModel = this.taskService.GetMTSBilling(fromDate, toDate, null, location);
                else
                    billingModel = this.taskService.GetMTSBilling(fromDate, toDate, User.Identity.Name, location);

                ViewBag.Message = "Result of " + DateTime.Now.ToShortDateString();
            }
            else
            {
                if (User.IsInRole(Constants.ROLE_ADMIN))
                    billingModel = this.taskService.GetMTSBilling(fromDate, toDate, null, location);
                else
                    billingModel = this.taskService.GetMTSBilling(fromDate, toDate, User.Identity.Name, location);

                if (!string.IsNullOrEmpty(toDate))
                    ViewBag.Message = "Result from " + fromDate + " to " + toDate;
                else
                    ViewBag.Message = "Result of " + fromDate;
            }

            Session["BillingModel"] = billingModel;
            ViewBag.locations = this.GetLocations();
            return View(billingModel);
        }

        [Authorize]
        public ActionResult ProjectWise()
        {
            ViewBag.Table = null;
            ViewBag.Locations = taskService.getlocations();
            return View();
        }


        public ActionResult AjaxProjectWise(string fromDate, string toDate, string location)
        {

            ProjectModel projectModel = new ProjectModel();
            Session["ProjectModel"] = null;

            if (string.IsNullOrEmpty(fromDate))
                fromDate = DateTime.Now.ToShortDateString();
            if (string.IsNullOrEmpty(toDate))
                toDate = DateTime.Now.ToShortDateString();

            if (location == null || location == "")
            {
                location = "0";
            }
            projectModel.data = taskService.GetProjectWise(fromDate, toDate, Convert.ToInt32(location));
            ViewBag.Table = "test";
            ViewBag.Message = "Result from " + fromDate + " to " + toDate;
            return View("_PVProjectReport", projectModel);
        }


        public FileResult ExportToCSVCategory(string fromdate, string todate, string location)
        {
            CategoryWiseModel objcat = new CategoryWiseModel();
            DataTable dtResult = new DataTable();
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            if (location == null || location == "")
            {
                location = "0";
            }
            dtResult = taskService.getcategorydata(Convert.ToDateTime(fromdate), Convert.ToDateTime(todate), Convert.ToInt16(location));
            //objcat.data = taskService.getcategorydata(Convert.ToDateTime(fromdate),Convert.ToDateTime(todate),Convert.ToInt16(location));
            //byte[] bpdf = new ViewAsPdf("_PVcategoryreport", objcat).BuildPdf(this.ControllerContext);

            //string mimeType = "application/pdf";
            //string filename = "CategoryWiseReport" + System.DateTime.Now.ToShortDateString() + ".pdf";
            //return File(bpdf, mimeType, filename);
            string WriteValue = "";
            int i = 0;
            foreach (DataColumn z in dtResult.Columns)
            {

                //This will create your Headers
                if (i == 0)
                {
                    WriteValue += string.Format("\"{0}\"", z.ColumnName.ToString());
                    i = 1;
                }
                else
                {
                    WriteValue += "," + string.Format("\"{0}\"", z.ColumnName.ToString());
                }
            }

            streamWriter.WriteLine(WriteValue);



            foreach (DataRow r in dtResult.Rows)
            {

                WriteValue = "";
                i = 0;
                foreach (DataColumn z in dtResult.Columns)
                {
                    if (i == 0)
                    {
                        WriteValue += string.Format("\"{0}\"", r[z.ColumnName].ToString());
                        i = 1;
                    }
                    else
                    {
                        WriteValue += "," + string.Format("\"{0}\"", r[z.ColumnName].ToString());
                    }
                }

                streamWriter.WriteLine(WriteValue);

            }

            streamWriter.Flush();
            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "CategoryWiseReport - " + fromdate + " - " + todate + ".csv");
        }
        public FileResult ExportToCSVProject(string fromdate, string todate, string location)
        {
            ProjectModel projectModel = new ProjectModel();
            DataTable dtResult = new DataTable();
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            if (location == null || location == "")
            {
                location = "0";
            }
            dtResult = taskService.GetProjectWise(fromdate, todate, Convert.ToInt32(location));

            string WriteValue = "";
            string writeHeader = "";
            int i = 0;
            foreach (DataColumn z in dtResult.Columns)
            {
                //This will create your Headers
                if (i == 0)
                {
                    writeHeader += string.Format("{0}", z.ColumnName.ToString());
                    WriteValue += "Project type";
                    i = 1;
                }
                else
                {
                    if (z.ColumnName.ToString().Contains("|"))
                    {
                        var outval = z.ColumnName.ToString();
                        string[] array = new string[2];
                        array = outval.Split('|');
                        writeHeader += "," + string.Format("{0}", array[0].ToString());
                        WriteValue += "," + string.Format("{0}", array[1].ToString());
                    }
                    // WriteValue += "," + string.Format("{0}", z.ColumnName.ToString());
                }
            }
            writeHeader += "," + "Total";
            streamWriter.WriteLine(writeHeader);
            streamWriter.WriteLine(WriteValue);
            foreach (DataRow r in dtResult.Rows)
            {
                WriteValue = "";
                i = 0;
                foreach (DataColumn z in dtResult.Columns)
                {
                    if (i == 0)
                    {
                        WriteValue += string.Format("{0}", r[z.ColumnName].ToString());
                        i = 1;
                    }
                    else
                    {
                        WriteValue += "," + string.Format("{0}", r[z.ColumnName].ToString());
                    }
                }
                streamWriter.WriteLine(WriteValue);
            }
            streamWriter.Flush();
            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "ProjectWiseReport - " + fromdate + " - " + todate + ".csv");
        }

        public FileResult ExportToCSVActivity(string fromdate, string todate, string Project_ID, string location_ID)
        {
            ActivityWiseModel objcat = new ActivityWiseModel();
            DataTable dtResult = new DataTable();
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            if (location_ID == null || location_ID == "")
            {
                location_ID = "0";
            }
            if (Project_ID == null || Project_ID == "")
            {

                Project_ID = "0";
            }


            dtResult = taskService.getActivityData(fromdate, todate, Convert.ToInt16(Project_ID), Convert.ToInt16(location_ID));

            string WriteValue = "";
            int i = 0;
            foreach (DataColumn z in dtResult.Columns)
            {

                //This will create your Headers
                if (i == 0)
                {
                    WriteValue += string.Format("\"{0}\"", z.ColumnName.ToString());
                    i = 1;
                }
                else
                {
                    WriteValue += "," + string.Format("\"{0}\"", z.ColumnName.ToString());
                }
            }

            streamWriter.WriteLine(WriteValue);



            foreach (DataRow r in dtResult.Rows)
            {

                WriteValue = "";
                i = 0;
                foreach (DataColumn z in dtResult.Columns)
                {
                    if (i == 0)
                    {
                        WriteValue += string.Format("\"{0}\"", r[z.ColumnName].ToString());
                        i = 1;
                    }
                    else
                    {
                        WriteValue += "," + string.Format("\"{0}\"", r[z.ColumnName].ToString());
                    }
                }

                streamWriter.WriteLine(WriteValue);

            }

            streamWriter.Flush();
            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "ActivityWiseReport - " + fromdate + " - " + todate + ".csv");
        }

        [HttpGet]
        public ActionResult EmployeeActivityWiseSummaryReport()
        {
            ViewBag.locations = taskService.getlocations();
            ViewBag.projects = taskService.ProjectList();
            return View();
        }


        public ActionResult EmployeeActivityWiseSummaryReport(string fromDate, string toDate, string Project_ID, string Location_ID)
        {

            ViewBag.resultcount = false;
            ActivityWiseModel objact = new ActivityWiseModel();

            if (Location_ID == null || Location_ID == "")
            {
                Location_ID = "0";
            }
            if (Project_ID == null || Project_ID == "")
            {

                Project_ID = "0";
            }

            objact.data = taskService.getActivityData(fromDate, toDate, Convert.ToInt16(Project_ID), Convert.ToInt16(Location_ID));

            if (objact.data.Rows.Count == 0)
            {
                ViewBag.resultcount = true;
            }
            return PartialView("~/Views/Report/_PVActivityWiseReport.cshtml", objact);

        }

        [HttpGet]
        public ActionResult EmployeeActivityWiseDetailedReport()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            ViewBag.Employee = items;
            ViewBag.projects = taskService.ProjectList();
            return View();
        }

        public ActionResult EmployeeActivityWiseDetailedReport(string fromDate, string toDate, string Project_ID, string Employee_ID)
        {
            ViewBag.resultcount = false;
            ActivityWiseModel objact = new ActivityWiseModel();

            if (Employee_ID == null || Employee_ID == "")
            {
                Employee_ID = "0";

            }
            if (Project_ID == null || Project_ID == "")
            {
                Project_ID = "0";
            }
            objact.data = taskService.getEmployeeDetailedActivity(fromDate, toDate, Int16.Parse(Project_ID), Int16.Parse(Employee_ID));

            if (objact.data.Rows.Count == 0)
            {
                ViewBag.resultcount = true;
            }
            return PartialView("~/Views/Report/_PVEmployeeDetailedActivity.cshtml", objact);
        }

        public ActionResult GetEmployeeByProjectId(string projectId)
        {
            if (projectId == null || projectId == "")
            {
                projectId = "0";
            }
            DataTable dt = new DataTable();
            dt = taskService.getEmployee(Convert.ToInt16(projectId));
            string result = JsonConvert.SerializeObject(dt);
            return Json(result, JsonRequestBehavior.AllowGet);

        }
        public FileResult ExportToCSVDetailedActivity(string fromdate, string todate, string Project_ID, string Employee_ID)
        {
            ActivityWiseModel objcat = new ActivityWiseModel();
            DataTable dtResult = new DataTable();
            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);
            if (Employee_ID == null || Employee_ID == "")
            {
                Employee_ID = "0";
            }
            if (Project_ID == null || Project_ID == "")
            {
                Project_ID = "0";
            }
            dtResult = taskService.getEmployeeDetailedActivity(fromdate, todate, Convert.ToInt16(Project_ID), Convert.ToInt16(Employee_ID));

            string WriteValue = "";
            int i = 0;
            foreach (DataColumn z in dtResult.Columns)
            {

                //This will create your Headers
                if (i == 0)
                {
                    WriteValue += string.Format("\"{0}\"", z.ColumnName.ToString());
                    i = 1;
                }
                else
                {
                    WriteValue += "," + string.Format("\"{0}\"", z.ColumnName.ToString());
                }
            }

            streamWriter.WriteLine(WriteValue);



            foreach (DataRow r in dtResult.Rows)
            {

                WriteValue = "";
                i = 0;
                foreach (DataColumn z in dtResult.Columns)
                {
                    if (i == 0)
                    {
                        WriteValue += string.Format("\"{0}\"", r[z.ColumnName].ToString());
                        i = 1;
                    }
                    else
                    {
                        WriteValue += "," + string.Format("\"{0}\"", r[z.ColumnName].ToString());
                    }
                }

                streamWriter.WriteLine(WriteValue);

            }

            streamWriter.Flush();
            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "EmployeeDetailedReport - " + fromdate + " - " + todate + ".csv");
        }

        public FileResult ExportToCSVForEmployeeActivity()
        {
            string output = null;

            MemoryStream memoryStream = new MemoryStream();
            StreamWriter streamWriter = new StreamWriter(memoryStream);

            streamWriter.WriteLine("Date,UserName,Project,Hours");

            List<Task> tasks = Session["Tasks"] as List<Task>;

            if (tasks != null)
            {
                tasks.ForEach(x =>
                {
                    output = string.Format("{0},\"{1}\",{2},{3}", x.ExecutionDate.ToString("MM/dd/yyyy"), x.Employee.LastName + " " + x.Employee.FirstName, x.Project.Name, x.Hours);
                    streamWriter.WriteLine(output);
                });
            }

            streamWriter.Flush();
            memoryStream.Position = 0;

            return File(memoryStream, "text/csv", "Export.csv");

        }


        #endregion
    }
}