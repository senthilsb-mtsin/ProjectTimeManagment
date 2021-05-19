using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Security;
using TimeManagement.Domain;
using TimeManagement.Models;

namespace TimeManagment.Domain.CustomProviders
{
    public class CustomMembershipProvider : MembershipProvider
    {
        #region Variables

        private readonly int MIN_PASSWORD_LENGTH = 5;

        #endregion

        #region Overridden Methods


        /// <summary>
        /// Validate user information
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public override bool ValidateUser(string userName, string password)
        {
            if (String.IsNullOrEmpty(userName))
                throw new ArgumentException("Value cannot be null or empty.", "userName");

            if (String.IsNullOrEmpty(password))
                throw new ArgumentException("Value cannot be null or empty.", "password");

            using (PortalDBEntities db = new PortalDBEntities())
            {
                //Get the login information by user name
                Login login = db.Logins.SingleOrDefault(x => x.UserId.ToUpper().Equals(userName.ToUpper()));
                //Employee emp = db.Employees.SingleOrDefault(x => x.Active.Equals(0));
                Employee emp =db.Employees.Where(x => x.Id==login.Employee_Id ).FirstOrDefault();
                //if (emp.Active.Equals(0))
                    //return false;

                if (login == null)
                    return false;

                //Decript password
                string decryptedPassword = password;

                if (!login.Password.Equals(decryptedPassword))
                    return false;
                if (!emp.Active)
                    return false;

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MinRequiredPasswordLength
        {
            get
            {
                return MIN_PASSWORD_LENGTH;
            }
        }

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
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public DataSet TSWeeklyEmail(string SPName, string EmpId, string Date)
        {

            DataTable dt = new DataTable();
            DataTable dt1 = new DataTable();
            DataSet retVal = new DataSet();
            retVal.Tables.Add(dt);
            retVal.Tables.Add(dt1);


            using (PortalDBEntities db = new PortalDBEntities())
            {
                var entityConn = db.Database.Connection;
                var cmd = entityConn.CreateCommand();
                cmd.CommandText = SPName;
                cmd.Connection.Open();
                cmd.Parameters.Add(new SqlParameter("EMPLOYEEID", EmpId));
                cmd.Parameters.Add(new SqlParameter("Date", Date));
                cmd.CommandType = CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                retVal.Load(reader, LoadOption.OverwriteChanges, dt, dt1);
                cmd.Connection.Close();
            }
            return retVal;

        }
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            using (PortalDBEntities db = new PortalDBEntities())
            {
                //Get the login information by user name
                Login login = db.Logins.SingleOrDefault(x => x.UserId.ToUpper().Equals(username.ToUpper()));

                if (login == null)
                    return false;

                login.Password = newPassword;
                login.Status = (int)LoginStatus.EXISTING;

                db.Logins.Attach(login);
                db.Entry(login).State = EntityState.Modified;
                db.SaveChanges();

                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="deleteAllRelatedData"></param>
        /// <returns></returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernameToMatch"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <returns></returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            //var usersContext = new UsersContext();
            //var user = usersContext.GetUser(username);
            //if (user != null)
            //{
            //    var memUser = new MembershipUser("CustomMembershipProvider", username, user.UserID, user.UserEmailAddress,
            //                                                string.Empty, string.Empty,
            //                                                true, false, DateTime.MinValue,
            //                                                DateTime.MinValue,
            //                                                DateTime.MinValue,
            //                                                DateTime.Now, DateTime.Now);
            //    return memUser;
            //}

            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerUserKey"></param>
        /// <param name="userIsOnline"></param>
        /// <returns></returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Status Codes

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createStatus"></param>
        /// <returns></returns>
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}