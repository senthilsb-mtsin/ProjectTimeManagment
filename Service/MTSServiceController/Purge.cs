using MTSEntBlocks.DataBlock;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace ServiceController
{
    public partial class Purge : Form
    {
        private XmlDocument _xmldoc;
        int traceLogMaxcount = Convert.ToInt16(ConfigurationManager.AppSettings["PurgeTraceLogMaxCount"]); 
        private List<String> errorLog = new List<String>();
        string spName = string.Empty;
        public Purge()
        {
            InitializeComponent();
            string XMLFilePath = ConfigurationManager.AppSettings["PurgeXMLPath"]; 
            dateTimePicker.MaxDate = DateTime.Today;
            if (File.Exists(XMLFilePath))
            {
                _xmldoc = new XmlDocument();
                _xmldoc.Load(XMLFilePath);
            }
            else
                TraceLog(string.Format("'{0}' XML File does not exist.", XMLFilePath));
        }

        private void Purge_Load(object sender, EventArgs e)
        {
            string NoOfDays = _xmldoc.SelectSingleNode("//numberofdays").InnerText;
            int num = 0;
            int.TryParse(NoOfDays, out num);
            maskedDays.Text = num.ToString();
        }
        private void dateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            dateTimePicker.CustomFormat = "MM/dd/yyyy"; 
            int sDays = (DateTime.Today.Date - dateTimePicker.Value.Date).Days;
            maskedDays.Text = sDays.ToString();
        }
       
        private void ChangeButtonState(bool state)
        {
            btnPurge.Enabled = state;
            btnPurgeFile.Enabled = state;
        }

        private void btnPurge_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to  proceed?", "" + "Purge Data" + "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {   
                    this.Cursor = Cursors.WaitCursor;
                    ChangeButtonState(false);                    
                    foreach (XmlNode node in _xmldoc.SelectNodes("//storedprocedures/storedprocedure"))
                    {
                        spName = node.InnerText;
                        if (spName != string.Empty)
                        {
                            ExecuteSP(spName, dateTimePicker.Value);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                string[] purgeDatasplit;
                TraceLog(ex.StackTrace);
                purgeDatasplit = ex.Message.Split('.');
                TraceLog("Error occured in stored procedure " + spName + "." + Environment.NewLine + purgeDatasplit[0]);
            }
            finally
            {
                ChangeButtonState(true);
                this.Cursor = Cursors.Default;
            }
        }

        private void btnPurgeFile_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to  proceed?", "" + "Purge File" + "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Cursor = Cursors.WaitCursor;
                    ChangeButtonState(false);
                    PurgeFile();
                }

            }
            catch (Exception ex)
            {
                string[] purgeFilesplit;
                purgeFilesplit = ex.Message.Split('.');
                TraceLog(purgeFilesplit[0]);
            }
            finally
            {
                ChangeButtonState(true);
                this.Cursor = Cursors.Default;
            }
        }

        private static bool IsDirectoryEmpty(DirectoryInfo directory)
        {
            FileInfo[] files = directory.GetFiles();
            DirectoryInfo[] subdirs = directory.GetDirectories();
            return (files.Length == 0 && subdirs.Length == 0);
        }

        private bool PurgeFiles(DirectoryInfo directoryInfo, int days, string fileExt)
        {
            bool fileDeleted = false;
            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                if (fileExt.Split(',').Contains(file.Extension))
                {
                    if (file.LastWriteTime < DateTime.Now.Subtract(TimeSpan.FromDays(days)))
                    {
                        file.Delete();
                        fileDeleted = true;
                    }
                }
            }
            foreach (DirectoryInfo subfolder in directoryInfo.GetDirectories())
            {
                if (PurgeFiles(subfolder, days, fileExt))
                {
                    fileDeleted = true;
                }

                if (IsDirectoryEmpty(subfolder))
                {
                    subfolder.Delete();
                }
            }
            return fileDeleted;

        }

        private void PurgeFile()
        {

            foreach (XmlNode node in _xmldoc.SelectNodes("//folders/folder"))
            {
                string folderpath = node.SelectSingleNode("path").InnerText;
                string fileExtensions = node.SelectSingleNode("filetypes").InnerText;
                DirectoryInfo dirInfo = new DirectoryInfo(folderpath);
                int sDays = Convert.ToInt16(maskedDays.Text);

                TraceLog("File(s) purge started...");

                if (PurgeFiles(dirInfo, sDays, fileExtensions))
                {
                    TraceLog("File(s) purge finished.");
                }
                else
                {
                    TraceLog(string.Format("File(s) not found for given aging '{0}', days '{1}' and filetypes '{2}'  in the following path '{3}'.", dateTimePicker.Value.ToString("MM/dd/yyyy"), sDays, fileExtensions, folderpath));
                }

            }
        }       

       

        private void TraceLog(string msg)
        {            
            string dt = DateTime.Now.ToString("MM/dd/yyyy") + " - " + DateTime.Now.ToLongTimeString() + " :: ";
            errorLog.Insert(0, dt + msg);

            if (errorLog.Count > traceLogMaxcount)
            {
                errorLog.RemoveAt(errorLog.Count - 1);
            }

            txtLog.Clear();
            foreach (var str in errorLog)
            {
                txtLog.Text += string.Concat(str, Environment.NewLine);
            }

        }
        private void ExecuteSP(string spName, params object[] paramValues)
        {
            int sRetVal = 0;
            TraceLog("Stored Procedure " + spName + " is started...");        
            DbCommand cmd = DataAccess.ExecuteNonQueryCMD(spName, paramValues);
            sRetVal = Convert.ToInt16(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
            if (sRetVal == 0)
                TraceLog("Data not found for stored procedure " + spName + ".");
            if (sRetVal == 1)
                TraceLog("Stored Procedure " + spName + " is finished.");
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {   
            if (MessageBox.Show("Do you want to close the Purge?", "Purge", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            if (txtLog.Text.Length > 0)
            {
                if (MessageBox.Show("Do you want to clear the trace logs.", "Trace Log", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    txtLog.Clear();
                    errorLog.Clear();
                }

            }
        }

        private void maskedDays_TextChanged(object sender, EventArgs e)
        {
            int num = 0;
            int.TryParse(maskedDays.Text, out num);
            dateTimePicker.Value = DateTime.Today.Subtract(TimeSpan.FromDays(num));
            btnPurge.Enabled = true;
            btnPurgeFile.Enabled = true;
            if (maskedDays.Text == string.Empty)
            {
                maskedDays.Text = "0";
            }
        }
    }
}
