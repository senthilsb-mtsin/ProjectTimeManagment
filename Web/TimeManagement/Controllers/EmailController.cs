using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;

namespace TimeManagement.Controllers
{
    public class EmailController : BaseController
    {
        //
        // GET: /Email/

        DataSet dtEmailData;

       
    
        public ActionResult TSEntryNotificationWeekly()
        {
            ////ttp://localhost:49610/email/TSEntryNotificationWeekly?ID=EmpTimeSheetEntry|4|05/07/2015
          var param=HttpUtility.UrlDecode(Request.QueryString["ID"]);
          string[] sparr = param.ToString().Split('|');
          var res = this.MembershipService.WeeklyEmail(sparr[0].Trim(),sparr[1].Trim(),sparr[2].Trim());        
          ViewBag.FirstName = res.Tables[1].Rows[0]["FIRSTNAME"];
          ViewBag.LastName = res.Tables[1].Rows[0]["LASTNAME"];
          ViewBag.To = res.Tables[1].Rows[0]["To"];
          ViewBag.OvertimeTbl = res.Tables[0].AsEnumerable();
          return View();
        }

    }
   
}
