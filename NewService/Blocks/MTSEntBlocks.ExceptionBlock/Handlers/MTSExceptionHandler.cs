using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System;



namespace MTSEntBlocks.ExceptionBlock.Handlers
{
    public static class MTSExceptionHandler
    {

        #region Static Variables

        private static readonly ExceptionPolicyFactory _exceptionPolicyFactory;
        private static readonly ExceptionManager _exceptionManager;

        #endregion

        #region Constructor
        static MTSExceptionHandler()
        {
            SetLogWriter.Create();
            _exceptionPolicyFactory = new ExceptionPolicyFactory();
            _exceptionManager = _exceptionPolicyFactory.CreateManager();
        }

        #endregion

        public static void HandleException(ref Exception ex)
        {
            Exception exceptionToThrow = null;

            if (_exceptionManager.HandleException(ex, "MTSExceptionHandling", out exceptionToThrow))
            {
                if (exceptionToThrow == null)
                    throw ex;
                else
                    throw exceptionToThrow;
            }

        }

        //public static void LogMessage(string msg)
        //{
        //    bool logTracing = false;
        //    Boolean.TryParse(ConfigurationManager.AppSettings["IntellaLeeaseImportLog"].ToLower(), out logTracing);

        //    if (logTracing)
        //        BaseExceptionHandler.Write(msg, "ServiceLoader_Logger");
        //}
        //public static void Write(string msg, string handler)
        //{
        //    Logger.Write(msg, handler);
        //}


        ////Log Exceptions to DB
        //public static void UpdateExceptionToDB(System.Exception ex)
        //{
        //    try
        //    {
        //        string ApplicationName = Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        //        string ExceptionMessage = ex.Message;
        //        string StackTrace = ex.StackTrace;

        //        if (ExceptionMessage != null)
        //            ExceptionMessage = ExceptionMessage.Length > 500 ? ExceptionMessage.Substring(0, 500) : ExceptionMessage;

        //        if (StackTrace != null)
        //            StackTrace = StackTrace.Length > 1000 ? StackTrace.Substring(0, 1000) : StackTrace;

        //        MTSEntBlocks.DataBlock.DataAccess.ExecuteNonQuery("XC_ADD_EXCEPTION_LOG", ApplicationName, ExceptionMessage, StackTrace);
        //    }
        //    catch (Exception logException)
        //    {
        //        LogException(ref logException);
        //    }
        //}

        ////Log Exceptions to DB
        //public static void UpdateCustomExceptionToDB(string message, string stacktrace, string appName = null)
        //{
        //    try
        //    {
        //        string ApplicationName = appName;
        //        if (ApplicationName == null)
        //        {
        //            Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        //        }
        //        string ExceptionMessage = message;
        //        string StackTrace = stacktrace;

        //        if (ExceptionMessage != null)
        //            ExceptionMessage = ExceptionMessage.Length > 500 ? ExceptionMessage.Substring(0, 500) : ExceptionMessage;

        //        if (StackTrace != null)
        //            StackTrace = StackTrace.Length > 1000 ? StackTrace.Substring(0, 1000) : StackTrace;

        //        MTSEntBlocks.DataBlock.DataAccess.ExecuteNonQuery("XC_ADD_EXCEPTION_LOG", ApplicationName, ExceptionMessage, StackTrace);
        //    }
        //    catch (Exception logException)
        //    {
        //        LogException(ref logException);
        //    }
        //}
    }
}