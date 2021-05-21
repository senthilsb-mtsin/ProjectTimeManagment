using MTSEntBlocks.DataBlock;
using System.Data;
namespace ServiceLoader
{
    class ServiceLoaderDataAccess
    {
        public static DataTable GetServiceServiceConfig(string ServiceName)
        {
            return DataAccess.ExecuteDataTable("[IL].[GETSERVICECONFIGFORSERVICE]", ServiceName);
        }
        public static void UpdateServiceStatus(string ServiceName, short Status)
        {
            DataAccess.ExecuteNonQuery("[IL].[UPDATESERVICESTATUS]", ServiceName, Status);
        }

        public static DataTable GetTenantSpecificTime(string SPandParameter)
        {
            string[] sparr = SPandParameter.Split('|');
            if (sparr.Length > 1)
                return DataAccess.ExecuteDataTable(sparr[0].Trim(), sparr[1].Split(','));
            else
                return DataAccess.ExecuteDataTable(sparr[0].Trim(), null);
        }
    }
}
