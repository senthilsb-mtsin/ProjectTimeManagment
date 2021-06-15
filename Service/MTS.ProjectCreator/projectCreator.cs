﻿using ImapX;
using MTS.EmailManager.EmailDataAccess;
using MTS.ServiceBase;
using MTSEntBlocks.DataBlock;
using MTSEntBlocks.ExceptionBlock.Handlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTS.ProjectCreator
{
    public class ProjectCreator : IMTSServiceBase
    {
        private EmailDAL dataaccess = new EmailDAL();
        private string emailSubject;

        public bool DoTask()
        {
            try
            {
                return this.ReadProjectEmails();
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return false;
            }
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

        private int UpdateCloseProject(Dictionary<string, string> mailContent)
        {
            try
            {
                return DataAccess.ExecuteNonQueryWithReturnValue("MTS_CLOSEPROJECT", new object[1]
                {
                (object) mailContent["Description"],
                }); 
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -1;
            }
        }
        private int UpdateCreateProject(Dictionary<string, string> mailContent)
        {
            try
            {
                return DataAccess.ExecuteNonQueryWithReturnValue("MTS_PROJECTCREATOR", new object[4]
                {
                (object) mailContent["Customer Name"],
                (object) mailContent["Description"],
                (object) Convert.ToDouble(mailContent["Quote Amount"].Substring(1, mailContent["Quote Amount"].Length - 1)),
                (object) Regex.Replace(mailContent["TECH"], @"\s+", "")
               });
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -99;
            }

        }
        private bool ReadProjectEmails()
        {
            try
            {
                DataSet imapDetails = this.dataaccess.GetImapDetails();
                if (imapDetails == null || imapDetails.Tables.Count == 0)
                {
                    throw new Exception("Imap table is empty");
                }

                DataRow[] dataRowArray = imapDetails.Tables[0].Select("IMAPID=1");

                string host = dataRowArray[0]["IMAPClientHost"].ToString();
                int port = Convert.ToInt32(dataRowArray[0]["IMAPClientPort"]);
                bool enableSSL = Convert.ToBoolean(dataRowArray[0]["EnableSsl"]);
                string userName = dataRowArray[0]["UserName"].ToString();
                string password = DataAccess.ExecuteScalar("MTS_GETIMAPPASSWORD", (object[])null).ToString();

                string searchQuery = "OR (SUBJECT \"NEW PROJECT FOR TREX\") (SUBJECT \"CLOSE PROJECT FOR TREX\")";
                string createProjectSubject = "NEW PROJECT FOR TREX";
                string closeProjectSubject = "CLOSE PROJECT FOR TREX";

                ImapClient client = new ImapClient(host, enableSSL);

                if (client.Connect())
                {
                    if (client.Login(userName, password))
                    {
                        var ProjectsMessages = client.Folders.Inbox.Search(searchQuery, ImapX.Enums.MessageFetchMode.Basic);
                        foreach (var message in ProjectsMessages)
                        {
                            if (message.Subject.Equals(createProjectSubject) && !message.Seen)
                            {
                                Dictionary<string, string> mailContent = StringfyContent(message.Body.Text);
                                dataaccess.LogProjectDatails(new object[] { message.From.Address.ToString(), message.To[0].ToString(), message.Subject.ToString(), message.Date.ToString(), null, DateTime.Now.ToString(), 0, 1, null });
                                int result = UpdateCreateProject(mailContent);

                                switch (result)
                                {
                                    case 0:
                                        dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, DateTime.Now, DateTime.Now, 1, 1, null });
                                        break;
                                    case -1:
                                        dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, null, DateTime.Now, -1, 1, "Problem in CUSTOMER column" });
                                        SendErrorMail(1, userName, message.From.ToString(), "Problem in CUSTOMER column");
                                        break;
                                    case -2:
                                        dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, null, DateTime.Now, -1, 1, "Problem in TECH column" });
                                        SendErrorMail(1, userName, message.From.ToString(), "Problem in TECH column");
                                        break;
                                    default:
                                        dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, null, DateTime.Now, -1, 1, "SOMETHING WENT WRONG" });
                                        SendErrorMail(1, userName, message.From.ToString(), "SOMETHING WENT WRONG");
                                        break;
                                }
                            }
                            else if (message.Subject.Equals(closeProjectSubject) && !message.Seen)
                            {
                                dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, null, DateTime.Now, 1, 2, null });

                                Dictionary<string, string> mailContent = StringfyContent(message.Body.Text);
                                int res = UpdateCloseProject(mailContent);
                                if (res == 1)
                                {
                                    dataaccess.LogProjectDatails(new object[] { message.From.ToString(), message.To.ToString(), message.Subject.ToString(), message.Date, null, DateTime.Now, 0, 2, null });
                                }
                            }
                            message.Seen = true;
                        }
                    }
                    else
                    {
                        throw new Exception("Authentication failed");
                    }
                }
                else
                {
                    throw new Exception("Connection failed");
                }
                client.Disconnect();
                return true;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return false;
            }
        }

        private void SendErrorMail(int smtpId, string fromAddress, string toAddress, string error)
        {
            try
            {
                DataSet stmpDetails = this.dataaccess.GetSTMPDetails();
                if (stmpDetails == null || stmpDetails.Tables.Count == 0)
                    return;
                MailMessage message = new MailMessage();
                message.To.Add(toAddress.ToString());
                message.Subject = emailSubject;
                message.From = new System.Net.Mail.MailAddress(fromAddress); //to avoid name collision
                message.Body = $"Hello {fromAddress}, \n Your Request for project creation has been failed \n Error details - {error} \n Thank you!";
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
            }
        }

        private Dictionary<string, string> StringfyContent(string body)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();
            try
            {
                string[] lines = body.Split('\n');
                foreach (string line in lines)
                {
                    if (!line.Trim().Equals(""))
                    {
                        pairs.Add(line.Substring(line.IndexOf(".") + 1,
                            line.IndexOf("=") - line.IndexOf(".") - 2).Trim(),
                            line.Substring(line.IndexOf("=") + 1, line.Length - line.IndexOf("=") - 1).Trim());
                    }
                }

            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
            return pairs;
        }
    }
}
