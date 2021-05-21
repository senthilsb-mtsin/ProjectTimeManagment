using Microsoft.Practices.EnterpriseLibrary.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTSEntBlocks.DataBlock
{
    static class SetDataLogger
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
