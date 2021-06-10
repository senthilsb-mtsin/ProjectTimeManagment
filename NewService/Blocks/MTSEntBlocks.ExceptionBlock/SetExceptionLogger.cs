using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;

namespace MTSEntBlocks.ExceptionBlock
{
    public static class SetLogWriter
    {
        public static void Create()
        {
            try
            {
                if (Logger.Writer == null)
                    Logger.SetLogWriter(new LogWriterFactory().Create(), false);
            }
            catch (Exception ex)
            {
                Logger.SetLogWriter(new LogWriterFactory().Create(), false);
                //throw;
            }
        }
    }
}
