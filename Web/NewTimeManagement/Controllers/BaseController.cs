using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TimeManagement.Models;
using TimeManagment.Domain.Services;
using System.Web.Routing;
using TimeManagement.Domain.Services;

namespace TimeManagement.Controllers
{
    public abstract class BaseController : Controller
    {
        #region Variables

        public PortalDBContext db;
        public TaskService taskService { get; set; }
        public MembershipService MembershipService { get; set; }
        public FormsAuthenticationService AuthenticationService { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public string CurrentUserId
        {
            get
            {
                return HttpContext.User.Identity.Name;
            }
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestContext"></param>
        protected override void Initialize(RequestContext requestContext)
        {
            if (taskService == null)
                taskService = new TaskService();

            if (MembershipService == null)
                MembershipService = new MembershipService();

            if (AuthenticationService == null)
                AuthenticationService = new FormsAuthenticationService();

            if (db == null)
                db = new PortalDBContext();

            base.Initialize(requestContext);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}
