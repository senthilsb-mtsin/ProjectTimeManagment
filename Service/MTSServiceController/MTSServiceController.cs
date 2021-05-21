using MTSEntBlocks.DataBlock;
using ServiceController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Linq;

namespace MTSServiceController
{
    public enum InvokeType
    {
        [Description("Polling Every Minute")]
        Polling = 0,
        [Description("Runs Everyday at configured Time Below")]
        Timed = 1,
        [Description("Tenant Specific Timed")]
        TenantSpecific = 2
    }
    public partial class MTSServiceController : Form
    {
        bool IsSaved = false;
        int SelectedIndex = -1;
        const string cInstallFileName = @"\svcont.log";

        DataTable dtService;
        List<DataRow> _services;
        enum ServiceStatus
        { Running = 0, Stopped, Paused, NotInstalled }

        public MTSServiceController()
        {
            ResetConfigMechanism();
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "Services.config");
            dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);


            InitializeComponent();
            _services = dtService.Rows.Cast<DataRow>().ToList();
            List<System.ServiceProcess.ServiceController> sc = System.ServiceProcess.ServiceController.GetServices().Where(x => _services.Any(l => l.ItemArray[0].ToString() == x.ServiceName)).ToList();
            if (sc.Count == 0)
            {
                btnStartAll.Enabled = false;
            }
            else
            {
                btnStartAll.Enabled = true;
            }


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
                return base.EditValue(context, provider, value);
            }
        }
        #endregion
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartOrRunOnce(false);
        }

        private void btnRunOnce_Click(object sender, EventArgs e)
        {
            bool AllowExecuteOnce = true;

            if (Convert.ToString(dtService.Rows[lstServices.SelectedIndex]["ServiceParams"]).Contains("AlarmStoredprocedure"))
            {


                AllowExecuteOnce = MessageBox.Show("Run Once will generate extracts for all tenants. Do you want to continue?",
                                     "Confirm Run Once!!",
                                     MessageBoxButtons.YesNo) == DialogResult.Yes; ;
            }
            if (AllowExecuteOnce)
            {
                StartOrRunOnce(true);
            }
        }

        private void SetMinioParameters()
        {

            //DataTable configTemp = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);
            //string serviceName = Convert.ToString(((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"]);
            //string xmlParams = Convert.ToString(configTemp.Rows[lstServices.SelectedIndex]["ServiceParams"]);
            //var Params = XDocument.Parse(xmlParams).Descendants("add").Select(z => new { Key = z.Attribute("key").Value, Value = z.Value }).ToList();
            //string accessKey = Params.Find(f => f.Key == "AccessKey").Value;
            //string secretKey = Params.Find(f => f.Key == "SecretKey").Value;
            //string port = Params.Find(f => f.Key == "EndPoint").Value;
            //string configPath = Path.Combine(Environment.CurrentDirectory, "ImageServiceUtils", serviceName, "config.json");

            //if (File.Exists(configPath))
            //{
            //    string json = File.ReadAllText(configPath);
            //    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            //    jsonObj["credential"]["accessKey"] = accessKey;
            //    jsonObj["credential"]["secretKey"] = secretKey;
            //    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            //    File.WriteAllText(configPath, output);
            //    ServiceDataAccess dataAccess = new ServiceDataAccess();
            //    dataAccess.SetMinioCredentials(accessKey, secretKey, port);
            //}
            //else
            //{
            //    MessageBox.Show("Unable to find the config file.Reinstall the service!");
            //}
        }

        private void StartOrRunOnce(bool IsRunOnce)
        {

            //if (Convert.ToString(((DataRowView)(lstServices.SelectedItem)).Row["SERVICE_TYPE"]) == "2")
            //{
            //    SetMinioParameters();
            //}

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
                DisableStartAllButton();
                DisableStopAllButton();



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
                DisableStopAllButton();
                DisableStartAllButton();

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
            //Hide();
        }

        private void ServiceController_Load(object sender, EventArgs e)
        {
            ReadServiceConfigFromDB();

        }

        private void ReadServiceConfigFromDB()
        {
            try
            {
                dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);
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
                DisableButtons(false, false, true, false, false);
            }
            else
            {
                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in running state." + Environment.NewLine);
                        DisableButtons(false, true, false, false, false);
                        break;
                    case ServiceControllerStatus.Stopped:
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in stopped state." + Environment.NewLine);
                        DisableButtons(true, false, false, true, true);
                        break;
                    case ServiceControllerStatus.Paused:
                        DisableButtons(true, true, false, false, false);
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
                        Stopwatch endStopWatch = new Stopwatch();
                        endStopWatch.Start();
                        txtLog.AppendText(GetCurrentDateTime() + sc.ServiceName + " is in stop pending state. Service will stop in a moment." + Environment.NewLine);
                        while (ProcessStatusPending() == (int)ServiceControllerStatus.StopPending && endStopWatch.Elapsed < TimeSpan.FromSeconds(10))
                        {
                            this.Cursor = Cursors.WaitCursor;
                        }
                        if (ProcessStatusPending() == (int)ServiceControllerStatus.StopPending)
                        {
                            var process = Process.GetProcessesByName(sc.ServiceName).First();
                            process.Kill();
                        }
                        endStopWatch.Stop();
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
            dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);

            LoadServiceConfigurations(dtService.Rows[lstServices.SelectedIndex]);
        }

        private void CheckForSaveConfigurations(string ServiceName)
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
            dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);
            foreach (DataRow _lstServices in dtService.Rows)
            {
                if (_lstServices["ServiceName"].ToString() == ServiceName)
                {
                    LoadServiceConfigurations(_lstServices);
                    break;
                }

            }
        }


        private void MinIO(string type)
        {
            string objectStorePath = Path.Combine(Environment.CurrentDirectory, "ImageServiceUtils", "IntellaLend-Object-Store-Service.exe");
            txtLog.Text = GetCurrentDateTime() + $"Service {type}ation started";
            Process proc = new Process();
            proc.StartInfo.FileName = objectStorePath;
            proc.StartInfo.Arguments = $" {type}";
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();
            proc.WaitForExit(2000);
            bool isRunning = !proc.HasExited;
            if (isRunning)
                proc.Kill();
            txtLog.Text = GetCurrentDateTime() + $"Service {type}ation End";

            if (type == "install")
                CheckServiceStatus();
        }


        private void InstallService()
        {
            try
            {
                if (Convert.ToString(((DataRowView)(lstServices.SelectedItem)).Row["SERVICE_TYPE"]) == "2")
                {
                    MinIO("install");
                }
                else
                {

                    ServiceProcessInstaller ProcesServiceInstaller = new ServiceProcessInstaller();
                    ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                    InstallContext Context = new System.Configuration.Install.InstallContext();
                    string ServiceExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                            ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe");

                    if (File.Exists(ServiceExePath))
                    {
                        File.Delete(ServiceExePath);
                    }


                    File.Copy(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        "ServiceLoader.exe"),
                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                        ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe"));

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

                List<System.ServiceProcess.ServiceController> sc = System.ServiceProcess.ServiceController.GetServices().Where(x => _services.Any(l => l.ItemArray[0].ToString() == x.ServiceName)).ToList();
                if (sc.Count == 0)
                {
                    btnStartAll.Enabled = false;

                }
                else
                {
                    btnStartAll.Enabled = true;
                }
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
                if (Convert.ToString(((DataRowView)(lstServices.SelectedItem)).Row["SERVICE_TYPE"]) == "2")
                {
                    MinIO("uninstall");
                }
                else
                {
                    ServiceInstaller ServiceInstallerObj = new ServiceInstaller();
                    File.Delete(Environment.CurrentDirectory + cInstallFileName);
                    InstallContext Context = new System.Configuration.Install.InstallContext(Environment.CurrentDirectory + cInstallFileName, null);
                    ServiceInstallerObj.Context = Context;
                    ServiceInstallerObj.ServiceName = ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString();
                    ServiceInstallerObj.Uninstall(null);
                    WriteLogFromInstallLog();
                    string ServiceExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                                            ((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() + ".exe");

                    if (File.Exists(ServiceExePath))
                    {
                        File.Delete(ServiceExePath);
                    }
                }
                List<System.ServiceProcess.ServiceController> sc = System.ServiceProcess.ServiceController.GetServices().Where(x => _services.Any(l => l.ItemArray[0].ToString() == x.ServiceName)).ToList();
                if (sc.Count == 0)
                {
                    btnStartAll.Enabled = false;

                }
                else
                {
                    btnStartAll.Enabled = true;
                }

            }
            catch (Exception ex)
            {
                txtLog.Text = ex.StackTrace;
            }
        }

        private void DisableButtons(bool Start, bool Stop, bool Install, bool UnInstall, bool RunOnce)
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
                        DataAccess.ExecuteNonQueryCMD("MTS_UpdateServiceConfig", dr["SERVICENAME"], dr["SERVICEDISPLAYNAME"], dr["SERVICEDESCRIPTION"], intInvokeType, dr["SERVICEPARAMS"], dr["TIME"]);
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
        private void StopAllService()
        {
            string ServiceName;
            bool IsOneServiceStopped = false;
            System.ServiceProcess.ServiceController service;
            try
            {
                System.ServiceProcess.ServiceController[] controllers = System.ServiceProcess.ServiceController.GetServices();
                foreach (var _mtsService in lstServices.Items)
                {
                    ServiceName = ((DataRowView)(_mtsService)).Row["ServiceName"].ToString();

                    foreach (var _allService in controllers)
                    {
                        if (ServiceName == _allService.ServiceName && _allService.Status == ServiceControllerStatus.Running)
                        {
                            service = new System.ServiceProcess.ServiceController(_allService.ServiceName);
                            if (service.Status != ServiceControllerStatus.Stopped)
                            {
                                txtLog.AppendText(GetCurrentDateTime() + ServiceName + " is in stop pending state. Service will stop in a moment." + Environment.NewLine);
                                service.Stop();
                                txtLog.AppendText(GetCurrentDateTime() + ServiceName + " is stopped" + Environment.NewLine);
                                IsOneServiceStopped = true;
                                if (((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() == ServiceName)
                                    DisableButtons(true, false, false, true, true);
                            }

                        }

                    }

                }

                btnStartAll.Enabled = IsOneServiceStopped;
                btnStopAll.Enabled = !IsOneServiceStopped;

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
        private void StartAllService()
        {
            string ServiceName;
            bool IsOneServiceStarted = false;
            System.ServiceProcess.ServiceController service;
            try
            {
                System.ServiceProcess.ServiceController[] controllers = System.ServiceProcess.ServiceController.GetServices();


                foreach (var _mtsService in lstServices.Items)
                {
                    ServiceName = ((DataRowView)(_mtsService)).Row["ServiceName"].ToString();

                    foreach (var _allService in controllers)
                    {
                        if (ServiceName == _allService.ServiceName && _allService.Status == ServiceControllerStatus.Stopped)
                        {
                            txtLog.AppendText(GetCurrentDateTime() + ServiceName + " is in start pending state. Service will start in a moment." + Environment.NewLine);
                            service = new System.ServiceProcess.ServiceController(_allService.ServiceName);
                            service.Start();
                            txtLog.AppendText(GetCurrentDateTime() + ServiceName + " is started" + Environment.NewLine);
                            IsOneServiceStarted = true;
                            if (((DataRowView)(lstServices.SelectedItem)).Row["ServiceName"].ToString() == ServiceName)
                                DisableButtons(false, true, false, false, false);
                        }
                    }
                }
                btnStartAll.Enabled = !IsOneServiceStarted;
                btnStopAll.Enabled = IsOneServiceStarted;

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
        private void DisableStartAllButton()
        {

            dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);

            List<DataRowView> _services = lstServices.Items.Cast<DataRowView>().ToList();
            List<System.ServiceProcess.ServiceController> sc = System.ServiceProcess.ServiceController.GetServices().Where(x => _services.Any(l => l.Row["ServiceName"].ToString() == x.ServiceName && x.Status == ServiceControllerStatus.Stopped)).Select(k => k).ToList();
            if (sc.Count == 0)
            {
                btnStartAll.Enabled = false;
            }
            else
            {
                btnStartAll.Enabled = true;

            }
        }
        private void DisableStopAllButton()
        {

            dtService = DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIG", null);

            List<DataRowView> _services = lstServices.Items.Cast<DataRowView>().ToList();
            List<System.ServiceProcess.ServiceController> sc = System.ServiceProcess.ServiceController.GetServices().Where(x => _services.Any(l => l.Row["ServiceName"].ToString() == x.ServiceName && x.Status == ServiceControllerStatus.Running)).Select(k => k).ToList();
            if (sc.Count == 0)
            {
                btnStopAll.Enabled = false;
            }
            else
            {
                btnStopAll.Enabled = true;

            }
        }
        private void LoadServiceConfigurations(DataRow service)
        {
            PropertyGridServiceConfiguration propgridservice = new PropertyGridServiceConfiguration();
            if (service != null)
            {
                propgridservice.ServiceDisplayName = service["ServiceDisplayName"].ToString();
                propgridservice.ServiceDescription = service["ServiceDescription"].ToString();
                propgridservice.ServiceInvokeType = (InvokeType)Enum.Parse(typeof(InvokeType), service["ServiceInvokeType"].ToString());
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
            if (!ConfigurationManager.AppSettings.AllKeys.Contains("EnablePurge") || ConfigurationManager.AppSettings["EnablePurge"] == "false")
            {
                tbServicePurge.TabPages.Remove(tbPurge);
            }
            DisableStartAllButton();
            DisableStopAllButton();

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
            if (tbServicePurge.SelectedTab == tbPurge)
            {
                ServiceController.Purge purge = new ServiceController.Purge();
                purge.ShowDialog();
                tbServicePurge.SelectedTab = tbServiceController;
            }
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

        private void MTSServiceController_ResizeBegin(object sender, EventArgs e)
        {

        }

        private void MTSServiceController_Resize(object sender, EventArgs e)
        {
            //if (this.WindowState == FormWindowState.Minimized)
            //{
            //    Hide();
            //    mtsTrayIcon.Visible = true;
            //}
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //base.OnFormClosing(e);

            //if (e.CloseReason != CloseReason.UserClosing) return;

            //e.Cancel = true;
            //Hide();
            //mtsTrayIcon.Visible = true;
        }

        private void mtsTrayIconMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //if (MessageBox.Show("Closing this service controller will stop the Encompass DownLoad service.Are you sure you want exit?", "Service Controller", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
            //{
            //    this.Close();
            //    this.Dispose();
            //    Environment.Exit(0);
            //}         
        }

        private void mtsTrayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //bool isEncompassMonitor = Convert.ToBoolean(ConfigurationManager.AppSettings["EncompassMonitor"]);
            //if (isEncompassMonitor)
            //{
            //    timer1.Enabled = false;
            //    try
            //    {
            //        DataAccess.ExecuteNonQuery("MTS_[UPDATETIMERCOUNT]", DateTime.Now);
            //    }
            //    catch (Exception ex)
            //    {
            //        txtLog.AppendText(ex.ToString());
            //    }

            //    timer1.Enabled = true;
            //}
            //else
            //{
            //    timer1.Enabled = false;
            //}

        }


        private void button1_Click(object sender, EventArgs e)
        {
            StartAllService();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopAllService();
        }
    }
}
