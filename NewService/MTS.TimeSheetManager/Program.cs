using MTS.EmailManager.EmailDataAccess;
using MTS.ServiceBase;
using MTSEntBlocks.ExceptionBlock.Handlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using MTSEntBlocks.DataBlock;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;

namespace MTS.TimeSheetManager
{
    public class Program : IMTSServiceBase
    {
        private EmailDAL dataaccess = new EmailDAL();
        private string emailSubject;

        public bool DoTask()
        {
            try
            {
                return this.TimeSheetManager();
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
            return true;
        }

        public void OnStart(string Params)
        {
            var list = XDocument.Parse(Params).Descendants((XName)"add").Select(z => new
            {
                Key = z.Attribute((XName)"key").Value,
                Value = z.Value
            }).ToList();
            this.emailSubject = list.Find(f => f.Key == "Subject").Value;
        }
        
        private bool TimeSheetManager()
        {
            try
            {
                //get employees with location = MTS India and active = 1
                DataSet employees = this.dataaccess.GetMTSIndiaEmployees();

                if (employees == null || employees.Tables.Count == 0)
                {
                    return true;
                }
                List<DataRow> list = employees.Tables[0].AsEnumerable().ToList();

                foreach (DataRow row in list)
                {
                    DateTime endDate = DateTime.Now.AddDays(-1); //yesterday
                    DateTime startDate = DateTime.Now.AddMonths(-1).AddDays(-15); //day before one and half months

                    for (DateTime currDate = startDate; currDate <= endDate; currDate = currDate.AddDays(1))
                    {
                        //not a weekend day
                        if (!(currDate.DayOfWeek == DayOfWeek.Saturday || currDate.DayOfWeek == DayOfWeek.Sunday))
                        {
                            int employeeId = Convert.ToInt32(row[0]);
                            //get timesheet hour for current date and employee id
                            double hours = getHours(currDate, employeeId);
                            //not a valid hour
                            if (hours != 8.0 && hours != -99)
                            {
                                string mailBody = CreateMailBody(row[1].ToString(), row[2].ToString(), currDate.ToShortDateString(), hours);
                                if(mailBody != null) SendNotification(1, row[3].ToString(), mailBody);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return false;
            }
        }
        private double getHours(DateTime currDate, int employeeId)
        {
            try
            {
                return Convert.ToDouble(dataaccess.GetTimeSheetDetails(currDate.ToShortDateString(), employeeId));
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -99;
            }
        }

        private string CreateMailBody(string firstName, string lastName, string date, double hours)
        {
            try
            {
                string msg = "";
                if (hours == -1) //no data(timesheet not filled)
                {
                    msg = "You're timesheet is Not Filled for " + date;
                }
                else if (hours < 8.0)
                {
                    msg = "You're timesheet hour is less than 8 hours for " + date;
                }
                else if (hours > 8.0)
                {
                    msg = "You're timesheet hour is more than 8 hours for " + date;
                }

                return "<p>Hello <b>" + firstName + " " + lastName + ",</b><p>" + "<p>" + msg + "</p>" + "<p>Thank you!</p>";
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        private void SendNotification(int smtpId, string toAddress, string mailBody)
        {
            try
            {
                DataSet stmpDetails = this.dataaccess.GetSTMPDetails();
                if (stmpDetails == null || stmpDetails.Tables.Count == 0)
                    return;
                DataRow[] dataRowArray = stmpDetails.Tables[0].Select("SmtpId='" + smtpId + "'");
                MailMessage message = new MailMessage();
                message.To.Add(toAddress.ToString());
                message.Subject = emailSubject;
                message.From = new MailAddress(dataRowArray[0]["USERNAME"].ToString());
                message.Body = mailBody;
                message.IsBodyHtml = true;
                new SmtpClient(dataRowArray[0]["SMTPCLIENTHOST"].ToString(), Convert.ToInt32(dataRowArray[0]["SMTPCLIENTPORT"]))
                {
                    EnableSsl = Convert.ToBoolean(dataRowArray[0]["ENABLESSL"]),
                    Timeout = Convert.ToInt32(dataRowArray[0]["TIMEOUT"]),
                    DeliveryMethod = ((SmtpDeliveryMethod)Convert.ToInt32(dataRowArray[0]["SMTPDELIVERYMETHOD"])),
                    UseDefaultCredentials = Convert.ToBoolean(dataRowArray[0]["USEDEFAULTCREDENTIALS"]),
                    Credentials = ((ICredentialsByHost)new NetworkCredential(dataRowArray[0]["USERNAME"].ToString(), dataRowArray[0]["PASSWORD"].ToString(), dataRowArray[0]["DOMAIN"].ToString()))
                }.Send(message);
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
        }
    }
}
