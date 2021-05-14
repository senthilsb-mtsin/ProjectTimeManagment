using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeManagement.Models;

namespace TimeManagement.Controllers
{
    public class ForgetPasswordController : BaseController
    {
        // GET: ForgetPassword
        public ActionResult UserIdForForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgetPasswordEntry()
        {
            string userName = Request.Form["UserId"];
            Login login = this.db.Logins.Where(x => x.UserId.ToUpper().Equals(userName.ToUpper())).FirstOrDefault();
            if (login != null)
            {
                int employeeId = (int)login.Employee_Id;
                Employee employee = this.db.Employees.Where(x => x.Id == employeeId).FirstOrDefault();

                MTS_EMAILMASTER mtsEmailMaster = new MTS_EMAILMASTER();
                mtsEmailMaster.EMAILSP = $"{employee.Email}|{Encryption.RandomString(6)}|{employee.FirstName}|{employee.LastName}"; ;
                mtsEmailMaster.TEMPLATEID = 2;
                mtsEmailMaster.STATUS = 0;
                mtsEmailMaster.REQUESTTIME = DateTime.Now;

                this.db.MTS_EMAILMASTER.Add(mtsEmailMaster);
                this.db.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }
    }
}