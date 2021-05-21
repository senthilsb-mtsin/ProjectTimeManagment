using MTS.ServiceBase;
using MTSEntBlocks.DataBlock;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.LoggerBlock;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Timers;

namespace ServiceController
{

    public class ServiceExecuterrDataAccess
    {
        public static System.Data.DataTable GetServiceServiceConfig(string ServiceName)
        {
            return DataAccess.ExecuteDataTable("MTS_GETSERVICECONFIGFORSERVICE", ServiceName);
        }
        public static void UpdateServiceStatus(string ServiceName, short Status)
        {
            DataAccess.ExecuteNonQuery("MTS_UPDATESERVICESTATUS", ServiceName, Status);
        }

        public static System.Data.DataTable GetTenantSpecificTime(string SPandParameter)
        {
            string[] sparr = SPandParameter.Split('|');
            if (sparr.Length > 1)
                return DataAccess.ExecuteDataTable(sparr[0].Trim(), sparr[1].Split(','));
            else
                return DataAccess.ExecuteDataTable(sparr[0].Trim(), null);
        }
    }

    public class ServiceExecuter
    {
        private static System.Timers.Timer ServiceTimer;
        private static System.Timers.Timer TimerAlaram;
        private string SName;
        private Int16 Retrycount;
        private Int16 MaxErrorsCount;
        private int CurrentRetryCnt;
        private int CurrentMaxErrorsCnt;
        System.Data.DataTable dtServiceSchedule;
        object objService;
        Type ServiceBaseType;
        List<string> lstAlaram;
        AutoResetEvent workevent = new AutoResetEvent(true);

        public ServiceExecuter(string serviceName)
        {
            SName = serviceName;
            LoadServiceSchedule();
            LoadService();
        }

        public void Execute(bool IsExecuteOnce = false)
        {
            if (IsExecuteOnce)
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

        public void Stop()
        {
            // workevent.WaitOne();
            if (ServiceTimer != null)
                ServiceTimer.Stop();

            if (TimerAlaram != null)
                TimerAlaram.Stop();
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


        private void UpdateServiceStatus(string ServiceName, short Status)
        {
            ServiceExecuterrDataAccess.UpdateServiceStatus(ServiceName, Status);
        }

        private void InvokeImmediateTimer(object sender, ElapsedEventArgs e)
        {
            ServiceTimer.Enabled = false;
            DoTaskForService();
            ServiceTimer.Enabled = true;
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

        private bool LoadServiceSchedule()
        {
            try
            {
                dtServiceSchedule = null;
                dtServiceSchedule = ServiceExecuterrDataAccess.GetServiceServiceConfig(SName);
                if (dtServiceSchedule.Rows.Count == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {

                MTSExceptionHandler.HandleException(ref ex);
                throw ex;
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

        private void LogDebugMessage(string msg)
        {
            SName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Logger.WriteTraceLog(SName + ":" + msg);
        }

        private void LogMessage(string msg)
        {
            SName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
            Logger.WriteTraceLog(SName + ":" + msg);
        }

    }
}
