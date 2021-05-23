using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MTSEntBlocks.DataBlock
{
    class TraceLogger
    {
        //private static LogWriter logWriter;
        private static string _methodName;

        public TraceLogger([CallerMemberName] string methodName = "")
        {
            _methodName = methodName;
            //logWriter = new LogWriterFactory().Create();
            SetDataLogger.Create();
        }

        //public static void Log(string message,
        //[CallerMemberName] string memberName = "",
        //[CallerFilePath] string sourceFilePath = "",
        //[CallerLineNumber] int sourceLineNumber = 0)        
        //{

        //}

        /// <summary>
        /// This is private helper method
        /// </summary>
        /// <param name="message">Message to be Logged</param>
        /// <param name="procedureOrQuery">Stored Procedure Name</param>
        /// <param name="parameterValues">Parameters if present</param>        
        public void Log(string message, string procedureOrQuery = "", params object[] parameterValues)
        {
            bool _DataLogTrace = false;
            
            Boolean.TryParse(ConfigurationManager.AppSettings["DataLogTrace"], out _DataLogTrace);

            if (_DataLogTrace)
            {
               

                StringBuilder _logTraces = new StringBuilder();

                _logTraces.AppendLine();
                _logTraces.AppendLine(string.Format("Method Name : {0}, APPMessage : {1}", _methodName, message));
                _logTraces.AppendLine(string.Format("Stored Procedure/SQL Query : {0}", procedureOrQuery));
                if (parameterValues != null)
                    _logTraces.Append(string.Format("Parameters : {0}", string.Join(",", parameterValues)));

                Logger.Write(_logTraces.ToString());
            }
        }

    }
}
