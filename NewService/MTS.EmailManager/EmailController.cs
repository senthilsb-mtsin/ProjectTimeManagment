using MTS.EmailManager.EmailDataAccess;
using MTS.ServiceBase;
using MTSEntBlocks.ExceptionBlock.Handlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace MTS.EmailManager
{
    public class EmailController : IMTSServiceBase
    {
        private EmailDAL dataaccess = new EmailDAL();
        private const string cImmediate = "1";
        private const string cDaily = "2";
        private const string cWeekly = "3";
        private const string cMonthly = "4";
        private const int cStatusSucess = 1;
        private const int cStatusFailure = 2;
        private DataSet dsSchedule;
        private DataSet dsTimeScheduler;
        private DataSet dsEmailMaster;
        private DataSet dsEmailTemplates;
        private Dictionary<long, TimeSpan> dictAlaram;

        public void OnStart(string Params)
        {
            this.LoadAlarmList();
            this.GetEmailSchedule();
            this.GetEmailTemplates();
        }

        public bool DoTask()
        {
            try
            {
                this.SendEmail();
                return true;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return false;
            }
        }

        public void SendEmail()
        {
            List<long> longList = new List<long>();
            try
            {
                if (DateTime.Now.ToString("HH:mm") == "00:00")
                {
                    this.LoadAlarmList();
                    this.GetEmailSchedule();
                    this.GetEmailTemplates();
                }
                else
                {
                    foreach (KeyValuePair<long, TimeSpan> keyValuePair in this.dictAlaram)
                    {
                        if (DateTime.Now.TimeOfDay >= keyValuePair.Value)
                        {
                            this.SendScheduledEmail(keyValuePair.Key);
                            longList.Add(keyValuePair.Key);
                        }
                    }
                    foreach (long key in longList)
                        this.dictAlaram.Remove(key);
                }
                this.SendImmediateEmail();
            }
            finally
            {
            }
        }

        private void SendImmediateEmail()
        {
            this.GetWaitingEmailTobeSent();
            this.DoSendMailImmediateProcess();
        }

        private void SendScheduledEmail(long ScheduleID)
        {
            this.GetWaitingEmailTobeSent();
            this.DoSendMailScheduledProcess(this.GetTemplateIdFromSchedule(ScheduleID).ToString());
        }

        private void LoadAlarmList()
        {
            this.dictAlaram = (Dictionary<long, TimeSpan>)null;
            this.dictAlaram = new Dictionary<long, TimeSpan>();
            this.GetEmailScheduleForTimeScheduler();
            this.CheckForDailySendBy();
            this.CheckForWeeklySendBy();
            this.CheckForMonthlySendBy();
        }

        private void GetEmailSchedule()
        {
            this.dsSchedule = this.dataaccess.GetEmailSchedule();
        }

        private void GetEmailScheduleForTimeScheduler()
        {
            this.dsTimeScheduler = this.dataaccess.GetEmailScheduleForTimeScheduler();
        }

        private void GetWaitingEmailTobeSent()
        {
            this.dsEmailMaster = this.dataaccess.GetWaitingEmailTobeSent();
        }

        private void GetEmailTemplates()
        {
            this.dsEmailTemplates = this.dataaccess.GetEmailTemplates();
        }

        private void DoSendMailImmediateProcess()
        {
            foreach (DataRow dataRow in this.dsSchedule.Tables[0].Select("SendBy=1"))
                this.DoEmailProcess(dataRow["TemplateId"].ToString());
        }

        private void DoSendMailScheduledProcess(string TemplateId)
        {
            this.DoEmailProcess(TemplateId);
        }

        private void DoEmailProcess(string TemplateId)
        {
            foreach (DataRow dataRow in this.dsEmailMaster.Tables[0].Select("TemplateId=" + TemplateId))
            {
                int Status = 1;
                try
                {
                    if (!this.SendMail(new EmailTemplate().CreateEmailTemplateAndProcess(this.dsEmailTemplates.Tables[0].Select("TemplateId=" + dataRow[nameof(TemplateId)].ToString())[0]["HTMLPAGE"].ToString(), dataRow["EmailSP"].ToString()), this.dsEmailTemplates.Tables[0].Select("TemplateId=" + dataRow[nameof(TemplateId)].ToString())[0]["SMTPID"].ToString()))
                        Status = 2;
                }
                catch (Exception ex)
                {
                    Status = 2;
                    MTSExceptionHandler.HandleException(ref ex);
                }
                this.UpdateEmailStatus((int)dataRow["ID"], Status);
            }
        }

        private DataSet GetDataFromEMailSP(string SPandParameter)
        {
            string[] strArray = SPandParameter.Split('|');
            return this.dataaccess.GetEmailDataFromSP(strArray[0].Trim(), (object[])strArray[1].Split(','));
        }

        private void UpdateEmailStatus(int Id, int Status)
        {
            this.dataaccess.UpdateEmailStatus(Id, Status);
        }

        private void CheckForDailySendBy()
        {
            foreach (DataRow dataRow in this.dsTimeScheduler.Tables[0].Select("sendby=2"))
                this.dictAlaram.Add((long)dataRow["ScheduleId"], (TimeSpan)dataRow["Time"]);
        }

        private void CheckForWeeklySendBy()
        {
            int TodayDayOfWeek = (int)DateTime.Now.DayOfWeek;
            foreach (DataRow dataRow in this.dsTimeScheduler.Tables[0].Select("sendby=3"))
            {
                if (((IEnumerable<string>)dataRow["Day"].ToString().Split(',')).ToList<string>().Exists((Predicate<string>)(x => x.Contains(TodayDayOfWeek.ToString()))))
                    this.dictAlaram.Add((long)dataRow["ScheduleId"], (TimeSpan)dataRow["Time"]);
            }
        }

        private void CheckForMonthlySendBy()
        {
            bool flag1 = false;
            bool flag2 = false;
            string DayOfMonth = DateTime.Now.Day.ToString();
            string str = DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month).ToString();
            if (DayOfMonth == str)
                flag1 = true;
            foreach (DataRow dataRow in this.dsTimeScheduler.Tables[0].Select("sendby=4"))
            {
                List<string> list = ((IEnumerable<string>)dataRow["Day"].ToString().Split(',')).ToList<string>();
                if (list.Exists((Predicate<string>)(y => y.Contains(DayOfMonth))))
                    flag2 = true;
                if (flag1 && (int)Convert.ToInt16(list[0]) > DateTime.Now.Day)
                    flag2 = true;
                if (flag2)
                    this.dictAlaram.Add((long)dataRow["ScheduleId"], (TimeSpan)dataRow["Time"]);
            }
        }

        private int GetTemplateIdFromSchedule(long ScheduleId)
        {
            return int.Parse(this.dataaccess.GetTemplateIDFromSchedule(ScheduleId).ToString());
        }

        private bool SendMail(Template email, string smtpId)
        {
            try
            {
                DataSet stmpDetails = this.dataaccess.GetSTMPDetails();
                if (stmpDetails == null || stmpDetails.Tables.Count == 0)
                    return false;
                MailMessage message = new MailMessage();
                message.To.Add(email.To);
                if (!string.IsNullOrEmpty(email.Cc))
                    message.To.Add(email.Cc);
                if (!string.IsNullOrEmpty(email.BCc))
                    message.To.Add(email.BCc);
                message.Subject = email.Subject;
                message.From = new MailAddress(email.From);
                message.Body = email.Body.Replace("\n", "");
                message.IsBodyHtml = true;
                DataRow[] dataRowArray = stmpDetails.Tables[0].Select("SmtpId='" + smtpId + "'");
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
                return false;
            }
            return true;
        }
    }
}
