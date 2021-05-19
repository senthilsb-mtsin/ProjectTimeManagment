using System.Web.Mvc;
using System.Web.Routing;
using TimeManagement.Domain.Services;
using TimeManagement.Models;
using TimeManagment.Domain.Services;

namespace TimeManagement.Controllers
{
    public abstract class BaseController : Controller
    {
        #region Variables

        public PortalDBEntities db;
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
                db = new PortalDBEntities();

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
