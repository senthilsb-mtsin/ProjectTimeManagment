using System;
using System.Configuration;
using System.Diagnostics;
using System.Text;

namespace MTSEntBlocks.LoggerBlock
{
    public static class DataTraceLogger
    {
        private static readonly bool _DataTraceLog;
        static DataTraceLogger()
        {
            //EntLogger.Logger.SetLogWriter(new EntLogger.LogWriterFactory().Create(), false);
            Boolean.TryParse(ConfigurationManager.AppSettings["DataTraceLog"], out _DataTraceLog);
        }
        /// <summary>
        ///   This is private helper method
        /// </summary>
        /// <param name="message">Message to be Logged</param>
        /// <param name="procedureOrQuery">Stored Procedure Name</param>
        /// <param name="parameterValues">Parameters if present</param>
        public static void Log(string message, string procedureOrQuery = "", params object[] parameterValues)
        {

            if (_DataTraceLog)
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine();
                sb.AppendLine(string.Format("Message : {0}", message));
                sb.AppendLine(string.Format("Stored Procedure/SQL Query : {0}", procedureOrQuery));
                if (parameterValues != null)
                    sb.Append(string.Format("Parameters : {0}", string.Join(",", parameterValues)));

                Logger.WriteTraceLog(sb.ToString(), "General", 5, 5000, TraceEventType.Critical);
            }
        }
    }
}

