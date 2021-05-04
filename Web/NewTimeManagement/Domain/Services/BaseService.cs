using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TimeManagement.Models;

namespace TimeManagement.Domain.Services
{
    public class BaseService
    {
        #region Variables

        public PortalDBContext db { get; set; }

        #endregion

        public BaseService()
        {
            this.db = new PortalDBContext();
        }
    }
}