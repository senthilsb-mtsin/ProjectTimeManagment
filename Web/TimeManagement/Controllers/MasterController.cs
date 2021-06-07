using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using TimeManagement.Models;
using TimeManagement.ViewModel;

namespace TimeManagement.Controllers
{
    public class MasterController : BaseController
    {

        #region Projects

        [HttpGet]
        public ActionResult GetProjects()
        {
            return View("Projects");
        }


        public ActionResult ResetPWD(int id)
        {
            Login existingTask = this.db.Logins.Where(x => x.Employee.Id.Equals(id)).FirstOrDefault();
            //Login existingTask = this.db.Logins.Find(id);           

            return PartialView("_PVResetPwdForEmployees", existingTask);
        }

        [HttpPost]
        public ActionResult ResetPwdForEmployees(Login model)
        {
            //string userName = Request.Form["UserName"];           
            string userName = model.UserId;
            string password = model.Password;
            // string confirmpassword = Request.Form["confirmPassword"];


            if (this.MembershipService.ChangePassword(userName, password))
            {
                ViewBag.Msg = "Success";
                //  return View("Employees");
                return Content("UPDATE_S");
            }
            else
            {
                TempData["error"] = "The user name or password provided is incorrect.";
                return RedirectToAction("Index");
            }
        }


        public ActionResult GetProject(jQueryDataTableParamModel param)
        {

            IQueryable<Project> tasks = this.db.Projects;

            if (tasks != null)
            {
                List<ProjectModel> result = new List<ProjectModel>();

                foreach (Project task in tasks)
                {
                    ProjectModel taskModel = new ProjectModel();
                    taskModel.DT_RowId = task.Id.ToString();
                    taskModel.projectcode = task.Number;
                    taskModel.projectname = task.Name;
                    taskModel.estdstartdate = task.EstStartDate != null ? task.EstStartDate.Value.ToString("MM/dd/yyyy") : "";
                    taskModel.estdenddate = task.EstEndDate != null ? task.EstEndDate.Value.ToString("MM/dd/yyyy") : "";
                    taskModel.Customer = task.Customer;
                    taskModel.TotalAmount = task.TotalAmount.ToString();
                    taskModel.Risk = task.Risk;
                    taskModel.Discount = task.Discount;
                    taskModel.MarginValue = task.MarginValue;
                    taskModel.DiscountAmount = task.DiscountAmount != null ? task.DiscountAmount.Value.ToString("00") : "";
                    taskModel.Completed = task.Completed;
                    taskModel.Te = task.Te.ToString();
                    taskModel.Tm = task.Tm.ToString();

                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                //  ViewData["Data"] = result;
                return Json(resultData, JsonRequestBehavior.AllowGet);
                //return View("Projects");-
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


        public ActionResult AddProject()
        {
            Project AddProj = new Project();


            IQueryable<Customer> tasks = this.db.Customers;
            List<SelectListItem> customers = new List<SelectListItem>();

            customers = (from dropdownfill in tasks.AsEnumerable().OrderBy(x => x.CustomerName)
                         select new SelectListItem     // data()
                         {
                             Value = dropdownfill.CustomerName,
                             Text = dropdownfill.CustomerName
                         }).ToList();

            ViewBag.CustomerList = customers;
            IQueryable<MTS_PROJECTTYPE> projectType = this.db.MTS_PROJECTTYPE;
            List<SelectListItem> Projtype = new List<SelectListItem>();
            Projtype = (from dropdownfill in projectType.AsEnumerable().OrderBy(x => x.Project_Type)
                        select new SelectListItem
                        {
                            Value = dropdownfill.Project_TypeID.ToString(),
                            Text = dropdownfill.Project_Type
                        }).ToList();
            ViewBag.ProjectTypeList = Projtype;

            IQueryable<Project> Ptasks = this.db.Projects;
            ProjectModel LastModel = new ProjectModel();

            if (Ptasks != null)
            {

                foreach (Project task in Ptasks)
                {
                    LastModel.projectcode = task.Number;
                }

            }


            AddProj.Number = (Convert.ToInt32(LastModel.projectcode) + 1).ToString();

            return PartialView("_PVAddProjects", AddProj);
        }

        [HttpPost]
        public ActionResult AddProject(Project model)
        {
            this.db.Projects.Add(model);
            this.db.SaveChanges();
            return RedirectToAction("GetProjects");
        }

        public ActionResult EditProject(int id)
        {
            IQueryable<Customer> tasks = this.db.Customers;
            List<SelectListItem> customers = new List<SelectListItem>();

            customers = (from dropdownfill in tasks.AsEnumerable().OrderBy(x => x.CustomerName)
                         select new SelectListItem     // data()
                         {
                             Value = dropdownfill.CustomerName,
                             Text = dropdownfill.CustomerName
                         }).ToList();

            ViewBag.CustomerList = customers;
            IQueryable<MTS_PROJECTTYPE> projectType = this.db.MTS_PROJECTTYPE;
            List<SelectListItem> Projtype = new List<SelectListItem>();
            Projtype = (from dropdownfill in projectType.AsEnumerable().OrderBy(x => x.Project_Type)
                        select new SelectListItem
                        {
                            Value = dropdownfill.Project_TypeID.ToString(),
                            Text = dropdownfill.Project_Type
                        }).ToList();
            ViewBag.ProjectTypeList = Projtype;

            Project existingTask = this.db.Projects.Find(id);

            return PartialView("_PVEditProjects", existingTask);
        }

        [HttpPost]
        public ActionResult EditProject(Project model)
        {

            Project existingproj = this.db.Projects.Where(x => x.Id.Equals(model.Id)).First();

            existingproj.Number = model.Number;
            existingproj.Name = model.Name;
            existingproj.EstStartDate = model.EstStartDate;
            existingproj.EstEndDate = model.EstEndDate;
            existingproj.Customer = model.Customer;
            existingproj.TotalAmount = model.TotalAmount;
            existingproj.Risk = model.Risk;
            existingproj.Discount = model.Discount;
            existingproj.MarginValue = model.MarginValue;
            existingproj.DiscountAmount = model.DiscountAmount;
            existingproj.Completed = model.Completed;
            existingproj.Te = model.Te;
            existingproj.Tm = model.Tm;
            existingproj.TypeOfProject = model.TypeOfProject;
            this.db.Projects.Attach(existingproj);
            db.Entry(existingproj).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetProjects");
        }


        [HttpPost]
        public ActionResult DeleteProject(Int64 id)
        {
            Project existingproj = this.db.Projects.Find(id);

            this.db.Projects.Remove(existingproj);
            this.db.SaveChanges();
            return RedirectToAction("GetProjects");

        }

        #endregion


        #region Customer

        [HttpGet]
        public ActionResult GetCustomers()
        {
            return View("Customers");
        }



        public ActionResult GetCustomer(jQueryDataTableParamModel param)
        {

            IQueryable<Customer> tasks = this.db.Customers;

            if (tasks != null)
            {
                List<CustomerModel> result = new List<CustomerModel>();

                foreach (Customer task in tasks)
                {
                    CustomerModel taskModel = new CustomerModel();
                    taskModel.DT_RowId = task.CustomerId;
                    taskModel.CustomerId = task.CustomerId;
                    taskModel.CustomerName = task.CustomerName;
                    taskModel.CustomerType = task.CustomerType;
                    taskModel.CustomerAddress1 = task.CustomerAddress1;
                    taskModel.CustomerAddress2 = task.CustomerAddress2;
                    taskModel.Description = task.Description;

                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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


        public ActionResult AddCustomer()
        {
            Customer AddCus = new Customer();
            return PartialView("_PVAddCustomer", AddCus);
        }

        [HttpPost]
        public ActionResult AddCustomer(Customer model)
        {
            this.db.Customers.Add(model);
            this.db.SaveChanges();
            return RedirectToAction("GetCustomers");
        }

        public ActionResult EditCustomer(int id)
        {
            Customer existingTask = this.db.Customers.Find(id);
            return PartialView("_PVEditCustomer", existingTask);
        }

        [HttpPost]
        public ActionResult EditCustomer(Customer model)
        {

            Customer existingCus = this.db.Customers.Where(x => x.CustomerId.Equals(model.CustomerId)).First();

            existingCus.CustomerId = model.CustomerId;
            existingCus.CustomerName = model.CustomerName;
            existingCus.CustomerType = model.CustomerType;
            existingCus.CustomerAddress1 = model.CustomerAddress1;
            existingCus.CustomerAddress2 = model.CustomerAddress2;
            existingCus.Description = model.Description;

            this.db.Customers.Attach(existingCus);
            db.Entry(existingCus).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetCustomers");
        }


        [HttpPost]
        public ActionResult DeleteCustomer(Int64 id)
        {
            Customer existingproj = this.db.Customers.Find(id);

            this.db.Customers.Remove(existingproj);
            this.db.SaveChanges();
            return RedirectToAction("GetCustomers");

        }

        #endregion

        #region Employees

        [HttpGet]
        public ActionResult GetEmployees()
        {
            return View("Employees");
        }



        public ActionResult GetEmployee(jQueryDataTableParamModel param)
        {

            IQueryable<Employee> tasks = this.db.Employees;

            if (tasks != null)
            {
                List<EmployeeModel> result = new List<EmployeeModel>();

                foreach (Employee task in tasks)
                {
                    EmployeeModel taskModel = new EmployeeModel();
                    int empid = task.Id;
                    taskModel.DT_RowId = task.Id;
                    int id = task.Id;
                   // Login logins = this.db.Logins.Where(x => x.Employee_Id == id).FirstOrDefault();
                    taskModel.DT_RowId = task.Id;
                    taskModel.Employeeid = task.Id;
                    Login login = this.db.Logins.Where(x => x.Employee_Id == empid).FirstOrDefault();
                    taskModel.LoginUserID = login.UserId;
                    taskModel.EmployeeCode = task.EmployeeCode;
                    taskModel.FirstName = task.FirstName;
                    taskModel.MiddleName = task.MiddleName;
                    taskModel.LastName = task.LastName;
                    taskModel.Suffix = task.Suffix;
                    taskModel.BillRate = task.BillRate.ToString();
                    taskModel.CompanyName = task.CompanyName;
                    taskModel.LocationId = task.Location.Name;
                    taskModel.Cost = task.Cost.ToString();
                    taskModel.DOB = task.DOB;
                    taskModel.DOJ = task.DOJ;
                    taskModel.Email = task.Email;
                    taskModel.Mobile = task.Mobile;
                    taskModel.Active = task.Active.ToString();
                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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


        public ActionResult AddEmployee()
        {
            IQueryable<Location> Loc = this.db.Locations;
            List<SelectListItem> Location = new List<SelectListItem>();

            Location = (from dropdownfill in Loc.AsEnumerable().OrderBy(x => x.Name)
                        select new SelectListItem     // data()
                        {
                            Value = dropdownfill.Id.ToString(),
                            Text = dropdownfill.Name
                        }).ToList();

            ViewBag.LocationList = Location;

            IQueryable<Role> roles = this.db.Roles;
            List<SelectListItem> emprole = new List<SelectListItem>();

            emprole = (from dropdownfill in roles.AsEnumerable().OrderBy(x => x.Name)
                       select new SelectListItem     // data()
                       {
                           Value = dropdownfill.Id.ToString(),
                           Text = dropdownfill.Name
                       }).ToList();

            ViewBag.RoleList = emprole;

            Employee AddEmp = new Employee();
            EmployeeRole emproles = new EmployeeRole();
            return PartialView("_PVAddEmployee", AddEmp);
        }

        [HttpPost]
        public ActionResult AddEmployee(Employee model, FormCollection fc)
        {
            Login login = model.Logins.FirstOrDefault();
            login.Password = Encryption.GetSHA1HashData(Encryption.CreateRandomPassword(6));
            login.Status = 2;

            Employee added = this.db.Employees.Add(model);

            EmployeeRole emprole = new EmployeeRole();
            emprole.RoleId = Convert.ToInt32(fc["role"]);
            emprole.EmployeeId = added.Id;

            this.db.Logins.Add(login);
            this.db.EmployeeRoles.Add(emprole);
            login.Employee = model;

            this.db.SaveChanges();
            return RedirectToAction("GetEmployees");
        }

        public ActionResult EditEmployee(int id)
        {
            IQueryable<Location> Loc = this.db.Locations;
            List<SelectListItem> Location = new List<SelectListItem>();

            Location = (from dropdownfill in Loc.AsEnumerable().OrderBy(x => x.Name)
                        select new SelectListItem     // data()
                        {
                            Value = dropdownfill.Id.ToString(),
                            Text = dropdownfill.Name
                        }).ToList();

            ViewBag.LocationList = Location;


            Employee existingTask = this.db.Employees.Find(id);
            return PartialView("_PVEditEmployee", existingTask);
        }

        [HttpPost]
        public ActionResult EditEmployee(Employee model)
        {

            Employee existingEmp = this.db.Employees.Where(x => x.Id.Equals(model.Id)).First();

            existingEmp.Id = model.Id;
            // existingEmp.Login.UserId = model.Login.UserId;
            existingEmp.EmployeeCode = model.EmployeeCode;
            existingEmp.FirstName = model.FirstName;
            existingEmp.MiddleName = model.MiddleName;
            existingEmp.LastName = model.LastName;
            existingEmp.Suffix = model.Suffix;
            existingEmp.BillRate = model.BillRate;
            existingEmp.CompanyName = model.CompanyName;
            existingEmp.Cost = model.Cost;
            existingEmp.DOB = model.DOB;
            existingEmp.DOJ = model.DOJ;
            existingEmp.Email = model.Email;
            existingEmp.Mobile = model.Mobile;
            existingEmp.LocationId = model.LocationId;
            existingEmp.DOR = model.DOR;
            existingEmp.EmailReminder = model.EmailReminder;
            existingEmp.Active = model.Active;
            this.db.Employees.Attach(existingEmp);
            db.Entry(existingEmp).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetEmployees");
        }


        [HttpPost]
        public ActionResult DeleteEmployee(Int64 id)
        {
            Employee existingEmp = this.db.Employees.Find(id);
            this.db.Employees.Remove(existingEmp);
            this.db.SaveChanges();
            return RedirectToAction("GetEmployees");

        }


        public ActionResult AssignProject(int id)
        {
            IQueryable<Project> projects = this.db.Projects;
            IQueryable<EmployeeProject> Eprojects = this.db.EmployeeProjects;
            List<EmployeeModel> result = new List<EmployeeModel>();

            List<SelectListItem> empproj = new List<SelectListItem>();

            empproj = (from dropdownfill in projects.AsEnumerable()
                       select new SelectListItem     // data()
                       {
                           Value = dropdownfill.Id.ToString(),
                           Text = dropdownfill.Name
                       }).ToList();


            EmployeeProject proj = new EmployeeProject();
            proj.EmployeeId = id;


            List<SelectListItem> projassign = new List<SelectListItem>();
            List<SelectListItem> COMMprojassign = new List<SelectListItem>();
            projassign = (from dropdownfill in Eprojects.AsEnumerable()
                          join p in projects on dropdownfill.ProjectId equals p.Id
                          where (dropdownfill.EmployeeId == id)
                          select new SelectListItem
                          {
                              Value = p.Id.ToString(),
                              Text = p.Name
                          }).ToList();
            COMMprojassign = (from comdropdownfill in projects.AsEnumerable()
                              where (comdropdownfill.IsCommon == true)
                              select new SelectListItem
                              {
                                  Value = comdropdownfill.Id.ToString(),
                                  Text = comdropdownfill.Name
                              }).ToList();



            ViewBag.AssignedProjectList = projassign;

            var exisproject = empproj.Where(p => !projassign.Any(p2 => p2.Value == p.Value)).ToList();
            exisproject = exisproject.Where(p => !COMMprojassign.Any(p2 => p2.Value == p.Value)).ToList();
            ViewBag.ProjectList = exisproject;

            return PartialView("_PVAssignProject", proj);
        }

        [HttpPost]
        public ActionResult AssignProject(int EmployeeId, String AssignedProjects)
        {



            List<EmployeeProject> exproj = this.db.EmployeeProjects.Where(x => x.EmployeeId == EmployeeId).ToList();

            foreach (EmployeeProject proj in exproj)
            {
                this.db.EmployeeProjects.Remove(proj);
            }

            this.db.SaveChanges();

            foreach (string pid in AssignedProjects.Split(','))
            {
                EmployeeProject eproj = new EmployeeProject()
                {
                    EmployeeId = EmployeeId,
                    ProjectId = Convert.ToInt32(pid)
                };
                this.db.EmployeeProjects.Add(eproj);
            }
            this.db.SaveChanges();


            return View("Employees");
        }



        #endregion

       
        #region Location

        [HttpGet]
        public ActionResult GetLocations()
        {
            return View("Locations");
        }



        public ActionResult GetLocation(jQueryDataTableParamModel param)
        {

            IQueryable<Location> tasks = this.db.Locations;

            if (tasks != null)
            {
                List<LocationModel> result = new List<LocationModel>();

                foreach (Location task in tasks)
                {
                    LocationModel taskModel = new LocationModel();
                    taskModel.DT_RowId = task.Id.ToString();
                    taskModel.Id = task.Id;
                    taskModel.Name = task.Name;

                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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

        [HttpGet]
        public ActionResult AddLocation()
        {
            Location AddLoc = new Location();
            return PartialView("_PVAddLocations", AddLoc);
        }

        [HttpPost]
        public ActionResult AddLocation(Location model)
        {
            this.db.Locations.Add(model);
            this.db.SaveChanges();
            return RedirectToAction("GetLocations");
        }

        public ActionResult EditLocation(int id)
        {
            Location existingLoc = this.db.Locations.Find(id);
            return PartialView("_PVEditLocations", existingLoc);
        }

        [HttpPost]
        public ActionResult EditLocation(Location model)
        {

            Location existingLoc = this.db.Locations.Where(x => x.Id.Equals(model.Id)).First();

            existingLoc.Id = model.Id;
            existingLoc.Name = model.Name;


            this.db.Locations.Attach(existingLoc);
            db.Entry(existingLoc).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetLocations");
        }


        [HttpPost]
        public ActionResult DeleteLocation(Int64 id)
        {
            Location existingLoc = this.db.Locations.Find(id);

            this.db.Locations.Remove(existingLoc);
            this.db.SaveChanges();
            return RedirectToAction("GetLocations");

        }

        #endregion



        #region WorkCodeActivity

        [HttpGet]
        public ActionResult GetWorkCodeActivity()
        {
            return View("WorkCodeActivity");
        }

        public ActionResult GetWorkCodeActivitys(jQueryDataTableParamModel param)
        {

            IQueryable<WorkCodesActivity> tasks = this.db.WorkCodesActivities;

            if (tasks != null)
            {
                List<WorkCodeActivityModel> result = new List<WorkCodeActivityModel>();

                foreach (WorkCodesActivity task in tasks)
                {
                    WorkCodeActivityModel taskModel = new WorkCodeActivityModel();
                    taskModel.DT_RowId = task.Id.ToString();
                    taskModel.id = task.Id;
                    taskModel.Name = task.Name;
                    taskModel.Number = task.Number;
                    taskModel.Billable = task.Billable.ToString();

                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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
        

        public ActionResult AddWorkCodeActivity()
        {

            IQueryable<WorkCodesActivity> tasks = this.db.WorkCodesActivities;
            WorkCodeActivityModel LastModel = new WorkCodeActivityModel();

            if (tasks != null)
            {

                foreach (WorkCodesActivity task in tasks)
                {
                    LastModel.Number = task.Number;
                }

            }

            WorkCodesActivity AddWork = new WorkCodesActivity();
            AddWork.Number = (Convert.ToInt32(LastModel.Number) + 1).ToString();
            return PartialView("_PVADDWorkcodeActivity", AddWork);
        }

        [HttpPost]
        public ActionResult AddWorkCodeActivity(WorkCodesActivity model)
        {
            this.db.WorkCodesActivities.Add(model);
            this.db.SaveChanges();
            return RedirectToAction("GetWorkCodeActivity");
        }

        public ActionResult EditWorkCodeActivity(int id)
        {
            WorkCodesActivity existingTask = this.db.WorkCodesActivities.Find(id);
            return PartialView("_PVEditWorkCodeActivity", existingTask);
        }

        [HttpPost]
        public ActionResult EditWorkCodeActivity(WorkCodesActivity model)
        {

            WorkCodesActivity existingwork = this.db.WorkCodesActivities.Where(x => x.Id.Equals(model.Id)).First();

            existingwork.Id = model.Id;
            existingwork.Name = model.Name;
            existingwork.Number = model.Number;
            existingwork.Billable = model.Billable;


            this.db.WorkCodesActivities.Attach(existingwork);
            db.Entry(existingwork).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetWorkCodeActivity");
        }
        #endregion


        #region WorkCode

        [HttpGet]
        public ActionResult GetWorkCodes()
        {
            return View("WorkCodes");
        }

        public ActionResult GetWorkCodesDetails(jQueryDataTableParamModel param)
        {

            IQueryable<WorkCode> tasks = this.db.WorkCodes;
            if (tasks != null)
            {
                List<WorkCodeModel> result = new List<WorkCodeModel>();

                foreach (WorkCode task in tasks)
                {
                    WorkCodeModel taskModel = new WorkCodeModel();
                    taskModel.Id = task.Id;
                    taskModel.Name = task.Name;
                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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

        public ActionResult AddWorkCodes()
        {
            WorkCode AddWork = new WorkCode();
           //AddWork.Id = (Convert.ToInt32(LastModel.Id) + 1);
            return PartialView("_PVAddWorkCodes", AddWork);
        }

        [HttpPost]
        public ActionResult AddWorkCodes(WorkCode model)
        {
            this.db.WorkCodes.Add(new WorkCode() { Name=model.Name });
            this.db.SaveChanges();
            return RedirectToAction("GetWorkCodes");
        }

        public ActionResult EditWorkCodes(int ? id)
        {
            WorkCode existingTask = this.db.WorkCodes.Find(id);
            return PartialView("_PVEditWorkCodes", existingTask);
        }

        [HttpPost]
        public ActionResult EditWorkCodes(WorkCode model)
        {
                WorkCode existingwork = this.db.WorkCodes.Where(x => x.Id == model.Id).FirstOrDefault();
                existingwork.Name = model.Name;
                db.Entry(existingwork).State = EntityState.Modified;
                this.db.SaveChanges();
                return RedirectToAction("GetWorkCodes");
        }



        public ActionResult AssignWorkCodeActivity(int id)
        {
            WorkCode proj = new WorkCode();
            proj.Id =  id;
            List<WorkCodesActivity> workCodeActivities = this.db.WorkCodesActivities.Where(x => x.WorkCodesId == 0 || x.WorkCodesId == id).ToList();
            ViewBag.AssignedProjectList =workCodeActivities.Where(x => x.WorkCodesId!=0).Select(x=>new SelectListItem(){ Value=x.Id.ToString(),Text=x.Name}).ToList();
            ViewBag.ProjectList = workCodeActivities.Where(x => x.WorkCodesId == 0).Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Name }).ToList();
            return PartialView("_PVAssignWorkCodeActivity", proj);
        }

        [HttpPost]
        public ActionResult AssignWorkCodeActivity(int  Id, String AssignedProjects)
        {

            try
            {
                List<WorkCodesActivity> workCodeActivities = this.db.WorkCodesActivities.Where(x => x.WorkCodesId == Id).ToList();
                foreach( WorkCodesActivity workcode in workCodeActivities)
                {
                    workcode.WorkCodesId = 0;
                    this.db.Entry(workcode).State = EntityState.Modified;
                    this.db.SaveChanges();
                }
                foreach (string activityId in AssignedProjects.Split(','))
                {
                    if (!string.IsNullOrEmpty(activityId.Trim()))
                    {
                        int activity = Convert.ToInt32(activityId);
                        WorkCodesActivity workCodeActivity = this.db.WorkCodesActivities.Where(x => x.Id == activity).FirstOrDefault();
                        if (workCodeActivity != null)
                        {
                            workCodeActivity.WorkCodesId = Id;
                            this.db.Entry(workCodeActivity).State = EntityState.Modified;
                            this.db.SaveChanges();
                        }
                    }
                }

                return Json(new { success = true });
            }
            catch(Exception e)
            {

                return Json(new { success = false });
            }
        }


        #endregion


        #region Configuration

        [HttpGet]
        public ActionResult GetConfigurations()
        {
            return View("Configurations");
        }

        public ActionResult GetConfiguration(jQueryDataTableParamModel param)
        {

            IQueryable<Configuration> tasks = this.db.Configurations;

            if (tasks != null)
            {
                List<ConfigurationModel> result = new List<ConfigurationModel>();

                foreach (Configuration task in tasks)
                {
                    ConfigurationModel taskModel = new ConfigurationModel();
                    taskModel.DT_RowId = task.Id.ToString();
                    taskModel.id = task.Id;
                    taskModel.Name = task.Name;
                    taskModel.Percentage = task.Percentage;
                    taskModel.Rate = task.Rate.ToString();

                    result.Add(taskModel);
                }

                var resultData = new
                {
                    aaData = result
                };
                return Json(resultData, JsonRequestBehavior.AllowGet);

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


        public ActionResult AddConfiguration()
        {
            Configuration AddWork = new Configuration();
            return PartialView("_PVAddConfigurations", AddWork);
        }

        [HttpPost]
        public ActionResult AddConfiguration(Configuration model)
        {
            this.db.Configurations.Add(model);
            this.db.SaveChanges();
            return RedirectToAction("GetConfigurations");
        }

        public ActionResult EditConfiguration(int id)
        {
            Configuration existingTask = this.db.Configurations.Find(id);
            return PartialView("_PVEditConfigurations", existingTask);
        }

        [HttpPost]
        public ActionResult EditConfiguration(Configuration model)
        {

            Configuration existingwork = this.db.Configurations.Where(x => x.Id.Equals(model.Id)).First();

            existingwork.Id = model.Id;
            existingwork.Name = model.Name;
            existingwork.Percentage = model.Percentage;
            existingwork.Rate = model.Rate;


            this.db.Configurations.Attach(existingwork);
            db.Entry(existingwork).State = EntityState.Modified;
            this.db.SaveChanges();
            return RedirectToAction("GetConfigurations");
        }


        [HttpPost]
        public ActionResult DeleteConfiguration(Int64 id)
        {
            Configuration existingwork = this.db.Configurations.Find(id);

            this.db.Configurations.Remove(existingwork);
            this.db.SaveChanges();
            return RedirectToAction("GetWorkCodes");

        }

        #endregion


    }
}

