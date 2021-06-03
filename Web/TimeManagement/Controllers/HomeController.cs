using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TimeManagement.Domain;
using TimeManagement.Models;
using TimeManagement.ViewModel;

namespace TimeManagement.Controllers
{
    public class HomeController : BaseController
    {
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("TimeEntry");
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AuthenticateUser()
        {
            string userName = Request.Form["UserId"];
            string password = Request.Form["Password"];
            

            Login login = this.db.Logins.Where(x => x.UserId.ToUpper().Equals(userName.ToUpper())).FirstOrDefault();

            //Reset password
            if ((login != null) && (login.Status.Value == (int)LoginStatus.NEW))
            {
                return RedirectToAction("ResetPassword", new { userName = userName });
            }

            if (this.MembershipService.ValidateUser(userName, password))
            {
                //Set authentication cookie
                AuthenticationService.SignIn(userName, false);

                return RedirectToAction("TimeEntry");
            }
            else
            {
                TempData["error"] = "The user name or password provided is incorrect.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangePassword()
        {
            string userName = User.Identity.Name;
            string password = Request.Form["Password"];
            // string confirmpassword = Request.Form["confirmPassword"];


            if (this.MembershipService.ChangePassword(userName, password))
            {
                //Set authentication cookie
                AuthenticationService.SignIn(userName, false);

                return RedirectToAction("TimeEntry");
            }
            else
            {
                TempData["error"] = "The user name or password provided is incorrect.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult ResetPassword(string userName)
        {
            ViewBag.userName = userName;

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Logoff()
        {
            this.AuthenticationService.SignOut();
            return RedirectToAction("Index");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult TimeEntry()
        {
            List<SelectListItem> Temp = null;

            //Get list of projects
            var employee = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee;
            List<Project> projects = employee.EmployeeProjects.Select(x => x.Project).ToList();

            //Get general projects
            List<Project> commonProjects = this.db.Projects.Where(x => x.IsCommon.Value == true).OrderBy(x => x.Name).ToList();
            projects.AddRange(commonProjects);

            if (projects != null)
            {
                Temp = new SelectList(projects, "Id", "Name").ToList();
                Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

                ViewBag.Projects = Temp;
            }

            //Get list of work codes
            IEnumerable<WorkCode> workCodes = this.db.WorkCodes.OrderBy(x => x.Name);

            Temp = new SelectList(workCodes, "Id", "Name").ToList();
            Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

            ViewBag.WorkCodes = Temp;
            Temp = new List<SelectListItem>();
            Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

            ViewBag.WorkCodesActivity = Temp;


            return View();
        }
        public JsonResult GetWorkCodeActivities(int? id)

        {
            List<Select2ListModel> Temp = new List<Select2ListModel>();
            if (id != null)
            {
                IEnumerable<WorkCodesActivity> workCodesActivity = this.db.WorkCodesActivities.Where(x => x.WorkCodeId == id).OrderBy(x => x.Name);
                Temp.Add(new Select2ListModel() { id = "", text = "" });
                foreach (var item in workCodesActivity)
                {
                    Temp.Add(new Select2ListModel() { id = item.Id.ToString(), text = item.Name });
                }
            }
            return Json(Temp, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult AddTask(Task task)
        {

            var message = "success";

            try
            {
                Login login = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).First();

                task.CreatedOn = DateTime.Now;
                task.EmployeeId = login.Employee.Id;
                task.Charge = login.Employee.BillRate.Value * task.Hours;
                if (!string.IsNullOrEmpty(task.Hours.ToString()))
                {
                    this.db.Tasks.Add(task);
                    this.db.SaveChanges();
                    return Json(new { message, task.Id });
                }
                return Json(new { });
            }
            catch (Exception ex)
            {

                message = ex.Message;
                return Json(new { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult EditTask(Task task)
        {
            var message = "success";

            try
            {
                Login login = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).First();

                Task existingTask = this.db.Tasks.Find(task.Id);
                existingTask.ExecutionDate = task.ExecutionDate;
                existingTask.ProjectId = task.ProjectId;
                existingTask.WorkCodeActivityId = task.WorkCodeActivityId;
                existingTask.Hours = task.Hours;
                existingTask.Description = task.Description;
                existingTask.Charge = login.Employee.BillRate.Value * existingTask.Hours;

                this.db.Tasks.Attach(existingTask);
                db.Entry(existingTask).State = EntityState.Modified;
                this.db.SaveChanges();

                return Json(new { message, task.Id });
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Json(new { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult RemoveTask(int taskId)
        {
            var message = "success";

            try
            {
                Task task = this.db.Tasks.Find(taskId);

                this.db.Tasks.Remove(task);
                this.db.SaveChanges();

                return Json(new { message });
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Json(new { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionDate"></param>
        /// <returns></returns>
        public ActionResult GetTasks(string executionDate)
        {
            int employeeId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee.Id;

            DateTime date = DateTime.ParseExact(executionDate, "MM/dd/yyyy", null);//Convert.ToDateTime(executionDate);

            IQueryable<Task> tasks = this.db.Tasks.Where(x => x.EmployeeId == employeeId && x.ExecutionDate == date);

            if (tasks != null)
            {
                List<TaskModel> result = new List<TaskModel>();

                foreach (Task task in tasks)
                {
                    TaskModel taskModel = new TaskModel();
                    taskModel.taskId = task.Id;
                    taskModel.executionDate = task.ExecutionDate.ToString("MM/dd/yyyy");
                    taskModel.project = task.Project.Name;
                    taskModel.workCodeId= task.WorkCodesActivity.Name;
                   taskModel.workCodeActivity = task.WorkCodesActivity.Name;
                    taskModel.hours = task.Hours.GetValueOrDefault();
                    taskModel.description = task.Description;

                    result.Add(taskModel);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
                return new JsonResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        public ActionResult GetTaskGroup(string fromDate, string toDate)
        {
            int employeeId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee.Id;

            DateTime fromdate = Convert.ToDateTime(fromDate);
            DateTime todate = Convert.ToDateTime(toDate);

            IQueryable<Task> tasks = this.db.Tasks.Where(x => x.EmployeeId == employeeId && x.ExecutionDate == fromdate);

            if (tasks != null)
            {
                List<TaskModel> result = new List<TaskModel>();

                foreach (Task task in tasks)
                {
                    TaskModel taskModel = new TaskModel();
                    taskModel.taskId = task.Id;
                    taskModel.executionDate = task.ExecutionDate.ToShortDateString();
                    taskModel.project = task.Project.Name;
                    taskModel.workCode = task.WorkCodesActivity.Name;
                    taskModel.hours = task.Hours.GetValueOrDefault();
                    taskModel.description = task.Description;

                    result.Add(taskModel);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
                return new JsonResult();
        }

        #endregion

        #region Status Report
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Status()
        {
            List<SelectListItem> Temp = null;

            //Get list of projects
            var employee = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee;
            List<Project> projects = employee.EmployeeProjects.Select(x => x.Project).OrderBy(x => x.Name).ToList();

            //Get general projects
            List<Project> commonProjects = this.db.Projects.Where(x => x.IsCommon.Value == true).ToList();
            projects.AddRange(commonProjects);

            if (projects != null)
            {
                Temp = new SelectList(projects, "Id", "Name").ToList();
                Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

                ViewBag.Projects = Temp;
            }

            //Get list of work codes
            IEnumerable<WorkCode> workCodes = this.db.WorkCodes.OrderBy(x => x.Name);

            Temp = new SelectList(workCodes, "Id", "Name").ToList();
            Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

            ViewBag.WorkCodes = Temp;

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult StatusReport()
        {
            List<SelectListItem> Temp = null;

            //Get list of projects
            var employee = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee;
            IEnumerable<Project> projects = employee.EmployeeProjects.Select(x => x.Project);

            if (projects != null)
            {
                Temp = new SelectList(projects, "Id", "Name").ToList();
                Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

                ViewBag.Projects = Temp;
            }

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionDate"></param>
        /// <returns></returns>
        public ActionResult GetStatusEntryReport(string fromDate, string toDate)
        {
            int employeeId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee.Id;

            DateTime Fromdate = Convert.ToDateTime(fromDate);
            DateTime Todate = Convert.ToDateTime(toDate);

            IQueryable<WeeklyReport> reports = this.db.WeeklyReports.Where(x => x.EmployeeId == employeeId && x.From >= Fromdate && x.To <= Todate);
            string userId = employeeId.ToString();

            //Get Notes
            //IEnumerable<Note> notes = this.db.Notes.Where(x => x.UserId == userId);

            if (reports != null)
            {
                List<StatusModel> result = new List<StatusModel>();

                foreach (WeeklyReport report in reports)
                {
                    StatusModel statusModel = new StatusModel();
                    statusModel.taskId = report.Id;
                    statusModel.From = report.From.ToShortDateString();
                    statusModel.To = report.To.ToShortDateString();
                    statusModel.Project = report.Project.Name;
                    statusModel.Scope = report.Scope;
                    statusModel.Schedule = report.Schedule;
                    statusModel.Quality = report.Quality;
                    statusModel.ClientSatisfaction = report.ClientSatisfaction;
                    statusModel.ProjectStatus = report.ProjectStatus.ToString();
                    statusModel.Risk = report.Risk.ToString();

                    //IQueryable<Note> notes = this.db.Notes.Where(x => x.UserId == employeeId.ToString() && x.Id == statusModel.taskId);
                    //foreach (Note note in notes)
                    //{
                    //    statusModel.Notes = note.WeeklyReport.ToString();
                    //}

                    result.Add(statusModel);
                }

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
                return new JsonResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult AddStatusReport(WeeklyReport report)
        {
            var message = "success";

            try
            {
                report.CreatedOn = DateTime.Now;
                report.EmployeeId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee.Id;

                //Note note = report.Notes.ToString();
                //this.db.Notes.Add(



                this.db.WeeklyReports.Add(report);
                //this.db.Notes.Add(report);

                this.db.SaveChanges();

                return Json(new { message, report.Id });
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Json(new { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult AddStatusReportNotes(Note note)
        {
            var message = "success";

            try
            {
                note.CreatedOn = DateTime.Now;
                note.UserId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().UserId;

                //Note note = report.Notes.ToString();
                //this.db.Notes.Add(



                this.db.Notes.Add(note);
                //this.db.Notes.Add(report);

                this.db.SaveChanges();

                return Json(new { message, note.Id });
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Json(new { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskId"></param>
        /// <returns></returns>
        [HttpPost, Authorize]
        public ActionResult RemoveStatusReport(int taskId)
        {
            var message = "success";

            try
            {
                WeeklyReport report = this.db.WeeklyReports.Find(taskId);

                this.db.WeeklyReports.Remove(report);
                this.db.SaveChanges();

                return Json(new { message });
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Json(new { message });
            }
        }

        #endregion

        #region Admin
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Admin()
        {
            List<SelectListItem> Temp = null;

            //Get list of projects
            var employee = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee;
            List<Project> projects = employee.EmployeeProjects.Select(x => x.Project).OrderBy(x => x.Name).ToList();

            var tasksgrp = from t in db.Tasks
                           join p in db.Projects on t.ProjectId equals p.Id
                           join w in db.WorkCodesActivities on t.WorkCodeActivityId equals w.Id
                           join e in db.Employees on t.EmployeeId equals e.Id
                           where t.Description == "vdsfgd"
                           select new { projectname = p.Name, workcode = w.Name, empname = e.LastName + " " + e.FirstName };

            foreach (var result in tasksgrp)
            {

            }

            //Get general projects
            List<Project> commonProjects = this.db.Projects.Where(x => x.IsCommon.Value == true).ToList();
            projects.AddRange(commonProjects);

            if (projects != null)
            {
                Temp = new SelectList(projects, "Id", "Name").ToList();
                Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

                ViewBag.Projects = Temp;
            }

            //Get list of work codes
            IEnumerable<WorkCode> workCodes = this.db.WorkCodes.OrderBy(x => x.Name);

            Temp = new SelectList(workCodes, "Id", "Name").ToList();
            Temp.Insert(0, new SelectListItem() { Text = "", Value = "" });

            ViewBag.WorkCodes = Temp;

            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionDate"></param>
        /// <returns></returns>
        public ActionResult GetAdminStatus(string fromDate, string toDate)
        {
            int employeeId = this.db.Logins.Where(x => x.UserId.Equals(this.CurrentUserId)).FirstOrDefault().Employee.Id;

            DateTime Fromdate = Convert.ToDateTime(fromDate);
            DateTime Todate = Convert.ToDateTime(toDate);

            IQueryable<Task> tasks = this.db.Tasks.Where(x => x.CreatedOn >= Fromdate && x.CreatedOn <= Todate);

            var tasksgrp = from t in db.Tasks
                           join p in db.Projects on t.ProjectId equals p.Id
                           join w in db.WorkCodes on t.WorkCodeActivityId equals w.Id
                           join e in db.Employees on t.EmployeeId equals e.Id
                           where t.ExecutionDate >= Fromdate && t.ExecutionDate <= Todate

                           select new { projectname = p.Name, workcode = w.Name, empname = e.LastName + " " + e.FirstName };


            List<TaskGroupModel> results = new List<TaskGroupModel>();

            if (tasksgrp != null)
            {
                foreach (var result in tasksgrp)
                {
                    TaskGroupModel taskgrpmodel = new TaskGroupModel();
                    taskgrpmodel.projectname = result.projectname;
                    taskgrpmodel.workcode = result.workcode;
                    taskgrpmodel.empname = result.empname;

                    results.Add(taskgrpmodel);
                }
                return Json(results, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return new JsonResult();
            }
        }

        #endregion

    }
}
