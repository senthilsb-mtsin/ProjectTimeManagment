using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeManagement.Models;
using System.Data.Entity;

namespace TimeManagement.Controllers
{
    public class ForgetPasswordController : BaseController
    {
        // GET: ForgetPassword
        public ActionResult AuthenticateUserId()
        {
            return View();
        }

        public JsonResult ForgetPasswordEntry(string userId)
        {
            Login login = this.db.Logins.Where(x => x.UserId.ToUpper().Equals(userId.ToUpper())).FirstOrDefault();
            if (login != null)
            {
                string password = Encryption.RandomString(6);
                login.Password = Encryption.GetSHA1HashData(password);
                int employeeId = (int)login.Employee_Id;
                Employee employee = this.db.Employees.Where(x => x.Id == employeeId).FirstOrDefault();

                MTS_EMAILMASTER mtsEmailMaster = new MTS_EMAILMASTER();
                mtsEmailMaster.EMAILSP = $"{employee.Email}|{password}|{employee.FirstName}|{employee.LastName}"; ;
                mtsEmailMaster.TEMPLATEID = 2;
                mtsEmailMaster.STATUS = 0;
                mtsEmailMaster.REQUESTTIME = DateTime.Now;

                this.db.Logins.Attach(login);
                db.Entry(login).State = EntityState.Modified;
                this.db.MTS_EMAILMASTER.Add(mtsEmailMaster);
                this.db.SaveChanges();

                TempData["success"] = "New password has been send to your email";
                var data = new { status = "success"};
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var data = new { status = "error" };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
        }
    }
}