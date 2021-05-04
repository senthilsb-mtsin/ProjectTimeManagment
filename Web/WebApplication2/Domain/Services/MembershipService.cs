using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using TimeManagment.Domain.CustomProviders;
using TimeManagement.Controllers;
using System.Data;

namespace TimeManagment.Domain.Services
{
    public class MembershipService
    {
        #region Variables

        private readonly CustomMembershipProvider _provider = new CustomMembershipProvider();
                
        #endregion 

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password)
        {
            return _provider.ValidateUser(userName, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(string userName, string newPassword)
        {
            return _provider.ChangePassword(userName, null, newPassword);
        }

        public DataSet WeeklyEmail(string SPName,string EmpId,string Date)
        {
            return _provider.TSWeeklyEmail(SPName,EmpId,Date);
        }
        #endregion

        
    }
}