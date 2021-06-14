using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Xml.Linq;
using System.Data.Common;
using MTSEntBlocks.DataBlock;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Configuration.Install;
using System.Globalization;
using System.Collections;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using ServiceController;
namespace MTSServiceController
{
    public enum InvokeType
    {
        [Description("Polling Every Minute")]
        Polling = 0,
        [Description("Runs Everyday at configured Time Below")]
        Timed = 1
    }
    public partial class MTSServiceController : Form
    {
        bool IsSaved = false;
        int SelectedIndex = -1;
        const string cInstallFileName = @"\svcont.log";

        DataTable dtService;       
        enum ServiceStatus
        { Running = 0, Stopped, Paused, NotInstalled }

        public MTSServiceController()
        {
            ResetConfigMechanism();
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "Services.config");
            InitializeComponent();  
        }

        private void ResetConfigMechanism()
        {
            typeof(ConfigurationManager)
                .GetField("s_initState", BindingFlags.NonPublic |
                                         BindingFlags.Static)
                .SetValue(null, 0);

            typeof(ConfigurationManager)
                .GetField("s_configSystem", BindingFlags.NonPublic |
                                            BindingFlags.Static)
                .SetValue(null, null);

            typeof(ConfigurationManager)
                .Assembly.GetTypes()
                .Where(x => x.FullName ==
                            "System.Configuration.ClientConfigPaths")
                .First()
                .GetField("s_current", BindingFlags.NonPublic |
                                       BindingFlags.Static)
                .SetValue(null, null);
        }

        #region Property Grid // Nov 28 2012
        [DefaultPropertyAttribute("ServiceDisplayName")]
        public class PropertyGridServiceConfiguration
        {
            private string _ServiceDisplayName;
            private string _ServiceDescription;
            private InvokeType _ServiceInvokeType;
            private EclipseServiceParams _ServiceParams;
            private string _Time;            
            // Name property with category attribute and 
            // description attribute added 
            [CategoryAttribute("Service Configuration"), DescriptionAttribute("Service Display Name")]
            [ReadOnly(true)]
            public string ServiceDisplayName
            {
                get
                {
                    return _ServiceDisplayName;
                }
                set
                {
                    _ServiceDisplayName = value;
                }
            }
            [CategoryAttribute("Service Configuration"),
           DescriptionAttribute("Service Description")]
            [ReadOnly(true)]
            public string ServiceDescription
            {
                get
                {
                    return _ServiceDescription;
                }
                set
                {
                    _ServiceDescription = value;
                }
            }
            [CategoryAttribute("Service Configuration"),
           DescriptionAttribute("Service Invoke Type")]
            public InvokeType ServiceInvokeType
            {
                get
                {
                    return _ServiceInvokeType;
                }
                set
                {
                    _ServiceInvokeType = value;
                }
            }          

            [CategoryAttribute("Service Configuration"), DescriptionAttribute("Service Params")]            
            public EclipseServiceParams ServiceParams
            {

                get
                {
                    return _ServiceParams;
                }
                set
                {
                    _ServiceParams = value;
                }

            }
            
            [CategoryAttribute("Service Configuration"), DescriptionAttribute("Time")]
                   
            public string Time
            {
                get
                {
                    return _Time;
                }
                set
                {  
                    _Time = value;
                }
            }
            public PropertyGridServiceConfiguration()
            {
                _ServiceParams = new EclipseServiceParams();
            }      

        }
        [Editor(typeof(ServiceParamsEditor), typeof(UITypeEditor))]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class EclipseServiceParams 
        {
            [ReadOnly(true)]
            public String ServiceParams { get; set; }
            public override string ToString()
            {
                
                return ServiceParams;
            }
        }

        class ServiceParamsEditor : UITypeEditor
        {
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.Modal;
            }

            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                IWindowsFormsEditorService scv = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                EclipseServiceParams esp = value as EclipseServiceParams;
                if (scv != null && esp != null)
                {
                    using (ServiceParamForm from = new ServiceParamForm())
                    {
                        from.XmlData = esp.ServiceParams;
                        if (scv.ShowDialog(from) == DialogResult.Yes)
                        {
                            EclipseServiceParams esparam = new EclipseServiceParams();
                            esparam.ServiceParams = from.XmlData;
                            return esparam;                            
                        }
                    }
                }
                return base.EditValue(context,provider,value);
            }
        }
        #endregion
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartOrRunOnce(false);
        }

        private void btnRunOnce_Click(object sender, EventArgs e)
        {
            StartOrRunOnce(true);
        }

        private void StartOrRunOnce(bool IsRunOnce)
        {
            string ServiceName;
            System.ServiceProcess.ServiceController service;
            try
            {
                    ChekForSaveConfigurations();
                    ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                    service = new System.ServiceProcess.ServiceController(ServiceName);
                    service.Start(new string[] { Convert.ToString(IsRunOnce) });
                    
                    if (IsRunOnce)
                    {
                        Cursor.Current = Cursors.WaitCursor; 

                        while (service.Status == ServiceControllerStatus.StartPending)
                        {
                            service.Refresh();
                        }

                        txtLog.AppendText(GetCurrentDateTime() + service.ServiceName + " is executed once sucessfully." + Environment.NewLine);
                        Application.DoEvents();
                        
                        service.Stop();

                        while ((service.Status == ServiceControllerStatus.Running || service.Status == ServiceControllerStatus.StopPending))
                        {
                            System.Threading.Thread.Sleep(10);
                            service.Refresh();
                        }

                        Cursor.Current = Cursors.Default;

                        CheckServiceStatus();
                    }
                    else
                    {
                        CheckServiceStatus();
                        txtLog.AppendText(GetCurrentDateTime() + service.ServiceName + " is started sucessfully." + Environment.NewLine);
                    }
                
            }
            catch (Exception ex)
            {
                txtLog.AppendText(ex.ToString());
            }
            finally
            {
                ServiceName = null;
                service = null;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            string ServiceName;
            System.ServiceProcess.ServiceController service;
            try
            {

                ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                service = new System.ServiceProcess.ServiceController(ServiceName);
                if (service.Status != ServiceControllerStatus.Stopped)
                    service.Stop();
                CheckServiceStatus();
                txtLog.AppendText(GetCurrentDateTime() + service.ServiceName + " is stopped" + Environment.NewLine);
            }
            catch (Exception ex)
            {
                txtLog.AppendText(ex.ToString());
            }
            finally
            {
                ServiceName = null;
                service = null;
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            InstallService();
            CheckServiceStatus();
        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            UnInstallService();
            CheckServiceStatus();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want exit?", "Service Controller", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
            {
                this.Close();
            }
        }

        private void ServiceController_Load(object sender, EventArgs e)
        {
            ReadServiceConfigFromDB();
        }

        private void ReadServiceConfigFromDB()
        {
            try
            {
                dtService = DataAccess.ExecuteDataTable("[dbo].[GETSERVICECONFIG]", null);
                lstServices.SelectedIndexChanged -= lstServices_SelectedIndexChanged;
                lstServices.DataSource = dtService.DefaultView;
                lstServices.SelectedIndexChanged += lstServices_SelectedIndexChanged;
                lstServices.DisplayMember = "ServiceName";
            }
            catch (Exception ex)
            {
                txtLog.AppendText(ex.ToString());
            }
        }

        private void CheckServiceStatus()
        {
            if (txtLog.Lines.Length > 200)
                txtLog.ResetText();

            System.ServiceProcess.ServiceController sc = System.ServiceProcess.ServiceController.GetServices().Where(s => s.ServiceName.ToUpper() == ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString().ToUpper()).FirstOrDefault();

            if (sc == null)
            {
                txtLog.AppendText(((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + " is not installed.");
                DisableButtons(false, false, true, false,false);
            }
            else
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in running state." + Environment.NewLine);
                        DisableButtons(false, true, false, false,false);
                        break;
                    case ServiceControllerStatus.Stopped:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in stopped state." + Environment.NewLine);
                        DisableButtons(true, false, false, true,true);
                        break;
                    case ServiceControllerStatus.Paused:
                        DisableButtons(true, true, false, false,false);
                        break;
                    case ServiceControllerStatus.StartPending:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in start pending state. Service will start in a moment." + Environment.NewLine);
                        while (ProcessStatusPending() == (int)ServiceControllerStatus.StartPending)
                        {
                            this.Cursor = Cursors.WaitCursor;
                        }
                        this.Cursor = Cursors.Default;
                        CheckServiceStatus();
                        break;
                    case ServiceControllerStatus.StopPending:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in stop pending state. Service will stop in a moment." + Environment.NewLine);
                        while (ProcessStatusPending() == (int)ServiceControllerStatus.StopPending)
                        {
                            this.Cursor = Cursors.WaitCursor;
                        }
                        this.Cursor = Cursors.Default;
                        CheckServiceStatus();
                        break;
                }
            }
        }

        private int ProcessStatusPending()
        {
            System.ServiceProcess.ServiceController scStatus = System.ServiceProcess.ServiceController.GetServices().Where(s => s.ServiceName.ToUpper() == ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString().ToUpper()).FirstOrDefault();
            return (int)scStatus.Status;
        }       

        private void lstServices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex == lstServices.SelectedIndex)
                return;
            txtLog.ResetText();
            CheckServiceStatus();
            SelectedIndex = lstServices.SelectedIndex;
            #region Property Grid // Nov 28 2012
            ChekForSaveConfigurations();
            #endregion

        }

        private void ChekForSaveConfigurations()
        {
            if (IsSaved)
            {
                if (MessageBox.Show("Do you want to save the changes?", "Service Controller", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                {
                    SaveServiceConfiguration();
                    MessageBox.Show("Service configurations updated successfully.");
                }
                else
                    IsSaved = false;
            }
            dtService = DataAccess.ExecuteDataTable("[dbo].[GETSERVICECONFIG]", null);

            LoadServiceConfigurations(dtService.Rows[lstServices.SelectedIndex]);
        }

        private void InstallService()
        {
            try
            {
                ServiceProcessInstaller ProcesServiceInstaller = new ServiceProcessInstaller();
                ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                InstallContext Context = new System.Configuration.Install.InstallContext();
                string ServiceExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
										((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe");

                if (!File.Exists(ServiceExePath))
                {
                    File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                        "ServiceLoader.exe"),
                                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), 
										((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe"));
                }

                String path = String.Format("/assemblypath={0}", ServiceExePath);
                if (!File.Exists(ServiceExePath))
                {
                    MessageBox.Show(((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe does not exists in " + ServiceExePath, "Service Controller", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                String[] cmdline = { path };
                Context = new System.Configuration.Install.InstallContext(Environment.CurrentDirectory + cInstallFileName, cmdline);
                ServiceInstallerObj.Context = Context;
                ServiceInstallerObj.DisplayName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceDisplayName"].ToString();
                ServiceInstallerObj.Description = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceDescription"].ToString(); ;
                ServiceInstallerObj.ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                ServiceInstallerObj.StartType = ServiceStartMode.Automatic;
                ServiceInstallerObj.Parent = ProcesServiceInstaller;

                System.Collections.Specialized.ListDictionary state = new System.Collections.Specialized.ListDictionary();
                ServiceInstallerObj.Install(state);
                WriteLogFromInstallLog();
            }
            catch (Exception ex)
            {
                txtLog.Text = ex.StackTrace;
            }
        }

        private void UnInstallService()
        {
            try
            {
                ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                File.Delete(Environment.CurrentDirectory + cInstallFileName);
                InstallContext Context = new System.Configuration.Install.InstallContext(Environment.CurrentDirectory + cInstallFileName, null);
                ServiceInstallerObj.Context = Context;
                ServiceInstallerObj.ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                ServiceInstallerObj.Uninstall(null);
                WriteLogFromInstallLog();
            }
            catch (Exception ex)
            {
                txtLog.Text = ex.StackTrace;
            }
        }

        private void DisableButtons(bool Start, bool Stop, bool Install, bool UnInstall,bool RunOnce)
        {
            btnStart.Enabled = Start;
            btnStop.Enabled = Stop;
            btnInstall.Enabled = Install;
            btnUninstall.Enabled = UnInstall;
            btnRunOnce.Enabled = RunOnce;
        }

        private void WriteLogFromInstallLog()
        {
            using (StreamReader reader = new StreamReader(Environment.CurrentDirectory + cInstallFileName))
            {
                txtLog.Text = GetCurrentDateTime() + reader.ReadToEnd();
            }
        }

        private string GetCurrentDateTime()
        {
            return DateTime.Now.ToShortDateString() + " - " + DateTime.Now.ToLongTimeString() + " :: ";
        }
        #region Property Grid // Nov 28 2012
        private void SaveServiceConfiguration()
        {   
                DataTable dtModified = dtService.GetChanges();
                IsSaved = false;                
                try
                {
                    if (dtModified != null)
                    {
                        foreach (DataRow dr in dtModified.Rows)
                        {
                            int intInvokeType = (int)Enum.Parse(typeof(InvokeType), dr["SERVICEINVOKETYPE"].ToString());
                            DataAccess.ExecuteNonQueryCMD("[dbo].[UpdateServiceConfig]", dr["SERVICENAME"], dr["SERVICEDISPLAYNAME"], dr["SERVICEDESCRIPTION"], intInvokeType, dr["SERVICEPARAMS"], dr["TIME"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update failed.");
                    txtLog.Text = ex.StackTrace;
                    tbCtrl.SelectedIndex = 0;
                }          
        }
        private void LoadServiceConfigurations(DataRow service)
        {
            PropertyGridServiceConfiguration propgridservice = new PropertyGridServiceConfiguration();
            if (service != null)
            {
                propgridservice.ServiceDisplayName = service["ServiceDisplayName"].ToString();
                propgridservice.ServiceDescription = service["ServiceDescription"].ToString();
                propgridservice.ServiceInvokeType = (InvokeType)Enum.Parse(typeof(InvokeType),service["ServiceInvokeType"].ToString());                
                propgridservice.ServiceParams.ServiceParams = service["ServiceParams"].ToString();
                propgridservice.Time = (service["Time"].ToString());
                propertyGrid1.SelectedObject = propgridservice;
            }
        }
        #endregion
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
            return xmlDocumentWithoutNs.ToString();
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }        

        private void MTSServiceController_Load(object sender, EventArgs e)
        {
            ReadServiceConfigFromDB();
        }

        private void MTSServiceController_KeyDown(object sender, KeyEventArgs e)
        {  
                if (e.Control && e.Shift && e.KeyCode == Keys.Q)
                {
                    ServiceController.DBQuery dbQryForm = new ServiceController.DBQuery();
                    dbQryForm.ShowDialog();
                }
        }       

        #region Property Grid // Nov 28 2012
        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)        
        {
            dtService.Rows[lstServices.SelectedIndex][propertyGrid1.SelectedGridItem.Label] = propertyGrid1.SelectedGridItem.Value;
            IsSaved = true;
            ChekForSaveConfigurations();
        }
        #endregion

        private void tbServicePurge_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (tbServicePurge.SelectedTab == tbPurge)
            //{
            //    ServiceController.Purge purge = new ServiceController.Purge();
            //    purge.ShowDialog();
            //    tbServicePurge.SelectedTab = tbServiceController;
            //}
        }

        
        #region "Pause Functionality - Commented"
        /*
            private void btnPause_Click(object sender, EventArgs e)
        {
            string ServiceName;
            System.ServiceProcess.ServiceController service;
            try
            {
                ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                service = new System.ServiceProcess.ServiceController(ServiceName);
                if (btnPause.Text == "Pause")
                {
                    service.Pause();
                    btnPause.Text = "Resume";
                    txtLog.AppendText(service.ServiceName + " is paused" + Environment.NewLine);
                }
                else if (btnPause.Text == "Resume")
                {
                    service.Continue();
                    btnPause.Text = "Resume";
                    txtLog.AppendText(service.ServiceName + " is resumed" + Environment.NewLine);
                }
                CheckServiceStatus();
            }
            finally
            {
                ServiceName = null;
                service = null;
            }
        }
         */
        #endregion


    }
}
