using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using TimeManagement.Models;

namespace TimeManagment.Domain.CustomProviders
{
    public class CustomRoleProvider : RoleProvider
    {
        #region Overridden Methods

        /// <summary>
        /// 
        /// </summary>
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="throwOnPopulatedRole"></param>
        /// <returns></returns>
        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="usernameToMatch"></param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override string[] GetRolesForUser(string userName)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Value cannot be null or empty.", "userName");

            List<string> roles = new List<string>();

            using (PortalDBContext db = new PortalDBContext())
            {
                Login login = db.Logins.FirstOrDefault(x => x.UserId.ToUpper().Equals(userName.ToUpper()));

                if (login == null)
                    return null;

                login.Employee.EmployeeRoles.ToList().ForEach(x => roles.Add(x.Role.Name));
            }

            return roles.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool IsUserInRole(string userName, string roleName)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Value cannot be null or empty.", "userName");

            using (PortalDBContext db = new PortalDBContext())
            {
                Login login = db.Logins.FirstOrDefault(x => x.UserId.ToUpper().Equals(userName.ToUpper()));

                if (login == null)
                    return false;

                EmployeeRole employeeRole = login.Employee.EmployeeRoles.FirstOrDefault(x => x.Role.Name.Equals(roleName));

                if (employeeRole != null)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernames"></param>
        /// <param name="roleNames"></param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}