using TimeManagement.Models;

namespace TimeManagement.Domain.Services
{
    public class BaseService
    {
        #region Variables

        public PortalDBEntities db { get; set; }

        #endregion

        public BaseService()
        {
            this.db = new PortalDBEntities();
        }
    }
}