using MTS.ServiceBase;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.LoggerBlock;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace ServiceLoader
{
    public partial class ServiceLoader : ServiceBase
    {
        private static System.Timers.Timer ServiceTimer;
        private static System.Timers.Timer TimerAlaram;
        private string SName;
        private Int16 Retrycount;
        private Int16 MaxErrorsCount;
        private int CurrentRetryCnt;
        private int CurrentMaxErrorsCnt;
        DataTable dtServiceSchedule;
        List<string> lstAlaram;
        object objService;
        Type ServiceBaseType;
        AutoResetEvent workevent = new AutoResetEvent(true);
        CancellationTokenSource serviceCancelTokenSource;
        List<Task> ServiceTaskList;
        public ServiceLoader()
        {
            ResetConfigMechanism();
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "Services.config");
            Environment.SetEnvironmentVariable("SERVICE_NAME", Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location));
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

        protected override void OnStart(string[] args)
        {
            // System.Diagnostics.Debugger.Launch();
            LogDebugMessage("OnStart Started");
            try
            {
                SName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                bool IsExecuteOnce = false;

                LogDebugMessage("SName " + SName);
                if (args.Length > 0)
                    IsExecuteOnce = Convert.ToBoolean(args[0].Trim());

                LoadServiceSchedule();
                LoadService();

                if (bool.Parse(ConfigurationManager.AppSettings["IsDebug"]) || IsExecuteOnce)
                {
                    LogDebugMessage("Debug Condition");
                    LogMessage("Run Once Start");

                    DoTaskForService();

                    LogMessage("Run Once End");
                }
                else
                {
                    LogDebugMessage("Timer Condition");
                    LogMessage("Start Service Start");
                    SetTimers();
                    UpdateServiceStatus(SName, 1);
                    LogMessage("Start Service End");
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                LogDebugMessage(ex.Message);
                LogDebugMessage(ex.StackTrace);
                MTSExceptionHandler.HandleException(ref ex);
                workevent.Set();
            }
        }

        private void LoadService()
        {
            try
            {
                string DLLName = dtServiceSchedule.Rows[0]["DLLName"].ToString();
                LogDebugMessage("Dllname - " + DLLName);
                string ServiceParams = dtServiceSchedule.Rows[0]["ServiceParams"].ToString();
                LogDebugMessage("Params - " + ServiceParams);
                Retrycount = Convert.ToInt16(dtServiceSchedule.Rows[0]["RetryCount"]);
                MaxErrorsCount = Convert.ToInt16(dtServiceSchedule.Rows[0]["MaxErrors"]);

                Assembly ServiceDll = Assembly.LoadFile(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\" + DLLName);

                LogDebugMessage("DLLLoad Path -" + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\" + DLLName);
                foreach (Type type in ServiceDll.GetTypes())
                {
                    if (typeof(IMTSServiceBase).IsAssignableFrom(type))
                    {
                        ServiceBaseType = type;
                        break;
                    }
                }

                LogDebugMessage("Instance Creation Started");
                objService = Activator.CreateInstance(ServiceBaseType);
                LogDebugMessage("Instance Creation Ended");

                LogDebugMessage("OnStart Method Invoke Started");
                MethodInfo method = ServiceBaseType.GetMethod("OnStart");
                method.Invoke(objService, new object[] { ServiceParams });
                LogDebugMessage("OnStart Method Invoke End");
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                LogDebugMessage(ex.Message);
                LogDebugMessage(ex.StackTrace);
                MTSExceptionHandler.HandleException(ref ex);
                workevent.Set();
            }
        }


        private void InvokeImmediateTimer(object sender, ElapsedEventArgs e)
        {
            ServiceTimer.Enabled = false;
            DoTaskForService();
            ServiceTimer.Enabled = true;
        }

        private void DoTaskForService(string param = "")
        {

            workevent.Reset();
            try
            {
                bool HasReachedMaxRetryCnt = false;

                LogMessage("Service Execution Started");
                LogDebugMessage("DoTask Method Call Started");

                object Status = new object();

                if (string.IsNullOrEmpty(param))
                {
                    Status = ServiceBaseType.InvokeMember("DoTask", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, objService, null);
                }
                else
                {
                    Status = ServiceBaseType.InvokeMember("DoTask", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, objService, new object[] { param });
                }

                LogDebugMessage("Status" + Status.ToString());
                LogMessage("Service Execution End Status:" + Status.ToString());
                LogDebugMessage("DoTask Method Call End");
                if (!Convert.ToBoolean(Status))
                {
                    if (CurrentMaxErrorsCnt == MaxErrorsCount)
                    {
                        this.Stop();
                        return;
                    }
                    if (CurrentRetryCnt == Retrycount)
                    {
                        ++CurrentMaxErrorsCnt;
                        HasReachedMaxRetryCnt = true;
                    }
                    if (!HasReachedMaxRetryCnt)
                    {
                        ++CurrentRetryCnt;
                        DoTaskForService(param);
                    }
                }
                CurrentRetryCnt = 0;
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                LogDebugMessage(ex.Message);
                LogDebugMessage(ex.StackTrace);
                MTSExceptionHandler.HandleException(ref ex);
                ++CurrentRetryCnt;
            }
            finally
            {
                workevent.Set();
            }
        }

        private void SetTimers()
        {


            try
            {
                double ServiceTimerInterval = 300000;
                int ServiceInvokeType = Convert.ToInt32(dtServiceSchedule.Rows[0]["SERVICEINVOKETYPE"]);

                if (ServiceInvokeType == 0)
                {
                    LogMessage("Service Invoke Type Polling");

                    ServiceTimerInterval = TimeSpan.FromMinutes(Convert.ToDouble(dtServiceSchedule.Rows[0]["Time"])).TotalMilliseconds;
                    LogMessage("Service will run every " + TimeSpan.FromMinutes(Convert.ToDouble(dtServiceSchedule.Rows[0]["Time"])).TotalMinutes.ToString() + " minutes");
                    ServiceTimer = new System.Timers.Timer(ServiceTimerInterval);
                    ServiceTimer.Elapsed += new ElapsedEventHandler(InvokeImmediateTimer);
                    ServiceTimer.Enabled = true;
                }
                else if (ServiceInvokeType == 1)
                {
                    LogMessage("Service Invoke Type Timed");
                    lstAlaram = new List<string>();
                    string timeDetails = (string)dtServiceSchedule.Rows[0]["Time"];
                    foreach (string time in timeDetails.Split(','))
                    {
                        LogMessage("Service Will run at " + time);
                        lstAlaram.Add(time);
                    }
                    TimerAlaram = new System.Timers.Timer(60000);
                    TimerAlaram.Elapsed += new ElapsedEventHandler(CheckTimeForLoadingAlarmList);
                    TimerAlaram.Enabled = true;
                }
                else if (ServiceInvokeType == 2)
                {
                    LogMessage("Service Invoke Type Tenant Specific");
                    CreateTenantSpecificTasks();
                    TimerAlaram = new System.Timers.Timer(60000);
                    TimerAlaram.Elapsed += new ElapsedEventHandler(InitTenantSpecificTimedService);
                    TimerAlaram.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                LogDebugMessage(ex.Message);
                LogDebugMessage(ex.StackTrace);
                MTSExceptionHandler.HandleException(ref ex);
                workevent.Set();
            }
        }

        #region TenantSpecific Timed Service

        private void InitTenantSpecificTimedService(object sender, ElapsedEventArgs e)
        {
            string currenttime = DateTime.Now.ToString("HH:mm");
            if (currenttime == "00:00") // Load alarmlist everyday at 12'O clock midnight
            {
                CreateTenantSpecificTasks();
                LogMessage("Service alarm list Refreshed");
            }
        }

        private Dictionary<Int64, List<TimeSpan>> GetTenantAlaramList()
        {
            string ServiceParams = dtServiceSchedule.Rows[0]["ServiceParams"].ToString();
            var Params = XDocument.Parse(ServiceParams).Descendants("add").Select(z => new { Key = z.Attribute("key").Value, Value = z.Value }).ToList();
            string AlaramListSp = Params.Find(f => f.Key == "AlarmStoredprocedure").Value;
            Dictionary<Int64, List<TimeSpan>> TenantAlaramList = new Dictionary<long, List<TimeSpan>>();
            DataTable timeDT = ServiceLoaderDataAccess.GetTenantSpecificTime(AlaramListSp);
            foreach (DataRow row in timeDT.Rows)
            {
                Int64 curTenant = Convert.ToInt64(row["TENANT_ID"]);

                List<TimeSpan> executionTimeList = new List<TimeSpan>();

                if (TenantAlaramList.Keys.Contains(curTenant))
                    executionTimeList = TenantAlaramList[curTenant];

                foreach (string time in Convert.ToString(row["EXECUTION_TIME"]).Trim().Split(','))
                {
                    if (time.Trim().Split('|').Length > 1)
                    {
                        if ((Convert.ToInt16(time.Trim().Split('|')[0]) - 1) == (int)DateTime.Now.DayOfWeek)
                        {
                            string weekTime = time.Trim().Split('|')[1];
                            foreach (string wtime in weekTime.Trim().Split(';'))
                            {
                                TimeSpan exeTime = new TimeSpan(Convert.ToInt32(wtime.Split(':')[0]), Convert.ToInt32(wtime.Split(':')[1]), 0);
                                if (!executionTimeList.Contains(exeTime))
                                    executionTimeList.Add(exeTime);
                            }
                        }
                    }
                    else
                    {

                        TimeSpan exeTime = new TimeSpan(Convert.ToInt32(time.Split(':')[0]), Convert.ToInt32(time.Split(':')[1]), 0);
                        if (!executionTimeList.Contains(exeTime))
                            executionTimeList.Add(exeTime);
                    }
                }

                executionTimeList.Sort();

                if (!TenantAlaramList.Keys.Contains(curTenant))
                    TenantAlaramList.Add(Convert.ToInt64(row["TENANT_ID"]), executionTimeList);
            }

            return TenantAlaramList;
        }

        private void CreateTenantSpecificTasks()
        {

            WaitForTenantSpecificTimedService();
            ServiceTaskList = new List<Task>();
            Dictionary<Int64, List<TimeSpan>> TenantAlaramList = GetTenantAlaramList();
            foreach (Int64 tenantID in TenantAlaramList.Keys)
            {
                LogMessage("Creating Tenant Specific Tasks for Tenant:" + tenantID.ToString());
                Int64 _tenantID = tenantID;
                serviceCancelTokenSource = new CancellationTokenSource();
                ServiceTaskList.Add(Task.Factory.StartNew((() => ServiceTask(_tenantID, TenantAlaramList[_tenantID], serviceCancelTokenSource.Token))));
            }
            LogMessage("Tenant Specific Tasks Created ");
        }

        private void WaitForTenantSpecificTimedService()
        {
            if (ServiceTaskList != null && ServiceTaskList.Count > 0 && serviceCancelTokenSource != null)
            {
                LogMessage("Waiting for Tenant Specific Tasks to complete");
                serviceCancelTokenSource.Cancel();
                foreach (Task task in ServiceTaskList)
                {
                    if (!task.IsCompleted)
                    {
                        task.Wait();
                    }
                }
                LogMessage("Tenant Specific Tasks are completed");
                workevent.Set();
            }
        }



        private void ServiceTask(Int64 TenantID, List<TimeSpan> executionTimeList, CancellationToken CToken)
        {
            foreach (TimeSpan time in executionTimeList.ToArray())
            {
                if (DateTime.Now.TimeOfDay > time)
                {
                    executionTimeList.Remove(time);
                }
            }

            string logMessage = "";
            foreach (TimeSpan time in executionTimeList.ToArray())
            {
                logMessage += " " + time.ToString();
            }
            LogDebugMessage("Task Created for Tenant " + TenantID.ToString() + " time:" + logMessage);


            foreach (TimeSpan executionTime in executionTimeList)
            {
                try
                {

                    LogDebugMessage("Task for Tenant:" + TenantID.ToString() + " will be start in " + executionTime.Subtract(DateTime.Now.TimeOfDay).TotalMinutes.ToString() + " minutes");
                    TimeSpan delayTime = executionTime.Subtract(DateTime.Now.TimeOfDay);
                    Task.Factory.StartNew((() => Thread.Sleep(delayTime))).Wait(CToken);
                }
                catch (System.OperationCanceledException ex) { LogDebugMessage("Task Cancelled"); }

                if (!CToken.IsCancellationRequested)
                {
                    LogDebugMessage("Task for Tenant:" + TenantID.ToString() + " execution started");
                    DoTaskForService(TenantID.ToString());
                    LogDebugMessage("Task for Tenant:" + TenantID.ToString() + " execution End");
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        protected override void OnStop()
        {
            WaitForTenantSpecificTimedService();
            workevent.WaitOne();
            UpdateServiceStatus(SName, 0);
        }

        private void UpdateServiceStatus(string ServiceName, short Status)
        {
            ServiceLoaderDataAccess.UpdateServiceStatus(ServiceName, Status);
        }

        private bool LoadServiceSchedule()
        {
            while (true)
            {
                try
                {
                    dtServiceSchedule = null;
                    dtServiceSchedule = ServiceLoaderDataAccess.GetServiceServiceConfig(SName);
                    if (dtServiceSchedule.Rows.Count == 0)
                        return false;
                    return true;
                }
                catch (Exception ex)
                {
                    Thread.Sleep(10000);
                    Exception exnew = new Exception("Exception while getting service schedule for the service.Will Retry after 10 sec", ex);
                    MTSExceptionHandler.HandleException(ref exnew);
                }

            }
        }

        private void CheckTimeForLoadingAlarmList(object sender, ElapsedEventArgs e)
        {
            TimerAlaram.Enabled = false;
            workevent.Reset();
            List<string> lstRemove = new List<string>();
            try
            {
                string currenttime = DateTime.Now.ToString("HH:mm");

                if (currenttime == "00:00") // Load alarmlist everyday at 12'O clock midnight
                {
                    LogMessage("Service Alarm List Refreshed");
                    lstAlaram = null;
                    SetTimers();
                }
                else
                {
                    foreach (var lsttime in lstAlaram)
                    {
                        if (currenttime == lsttime.ToString())
                        {
                            DoTaskForService();
                            lstRemove.Add(lsttime);
                        }
                    }
                    foreach (var remove in lstRemove)
                    {
                        lstAlaram.Remove(remove);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
                LogMessage(ex.StackTrace);
                LogDebugMessage(ex.Message);
                LogDebugMessage(ex.StackTrace);
                MTSExceptionHandler.HandleException(ref ex);
            }
            finally
            {
                workevent.Set();
                TimerAlaram.Enabled = true;
                lstRemove = null;
            }
        }

        private void LogMessage(string msg)
        {
            SName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Logger.WriteTraceLog(SName + ":" + msg);
        }

        private void LogDebugMessage(string msg)
        {
            SName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Logger.WriteTraceLog(SName + ":" + msg);
        }
    }
}
