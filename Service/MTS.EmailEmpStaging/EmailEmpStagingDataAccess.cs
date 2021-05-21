using MTSEntBlocks.DataBlock;

namespace MTS.EmailEmpStaging
{
    internal class EmailEmpStagingDataAccess
    {
        public static void DoEmailStaging(string EmailStagingSPName, string DayofWeek)
        {
            DataAccess.ExecuteNonQuery(EmailStagingSPName, new object[1]
            {
                (object) DayofWeek
            });
        }
    }
}
