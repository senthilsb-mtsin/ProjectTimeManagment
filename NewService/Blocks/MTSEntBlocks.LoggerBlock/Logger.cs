using MTSEntBlocks.ExceptionBlock;
using System;
using System.Diagnostics;
using EntLogging = Microsoft.Practices.EnterpriseLibrary.Logging;

namespace MTSEntBlocks.LoggerBlock
{
    public static class Logger
    {
        //private static readonly EntLogging.LogWriter logWriter;

        static Logger()
        {
            SetLogWriter.Create();
            //logWriter = new LogWriterFactory().Create();
            //Microsoft.Practices.EnterpriseLibrary.Logging.Logger.SetLogWriter(logWriter, false);
        }

        //public static Microsoft.Practices.EnterpriseLibrary.Logging.LogWriter LogWriter
        //{
        //    get
        //    {
        //        return logWriter;
        //    }
        //}
        public static void WriteTraceLog(String message)
        {
            EntLogging.Logger.Write(message, "General", 5, 2000, TraceEventType.Information);
        }

        public static void WriteTraceLog(String message, string category, int priority, int level, TraceEventType type)
        {
            EntLogging.Logger.Write(message, category, priority, level, type);
        }

        public static void WriteErrorLog(String message)
        {
            EntLogging.Logger.Write(message, "General", 5, 9999, TraceEventType.Error);
        }

    }


}
