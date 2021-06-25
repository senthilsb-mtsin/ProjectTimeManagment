using MTSEntBlocks.DataBlock;
using System.Data;

namespace MTS.EmailManager.EmailDataAccess
{
    public class EmailDAL
    {
        public DataSet GetEmailSchedule()
        {
            return DataAccess.ExecuteDataset("MTS_GetEmailSchedule", (object[])null);
        }

        public DataSet GetEmailScheduleForTimeScheduler()
        {
            return DataAccess.ExecuteDataset("MTS_GetEmailScheduleForTimeScheduler", (object[])null);
        }

        public DataSet GetWaitingEmailTobeSent()
        {
            return DataAccess.ExecuteDataset("MTS_GetEmailsWaitingToBeSend", (object[])null);
        }

        public DataSet GetEmailTemplates()
        {
            return DataAccess.ExecuteDataset("MTS_GetEmailTemplate", (object[])null);
        }

        public void UpdateEmailStatus(int Id, int Status)
        {
            DataAccess.ExecuteNonQuery("MTS_UpdateEmailStatus", new object[2]
            {
        (object) Id,
        (object) Status
            });
        }

        public int LogProjectDatails(object[] data)
        {
            return DataAccess.ExecuteNonQuery("MTS_EMAILLOGGERSP", data);
        }
        public DataSet GetImapDetails()
        {
            return DataAccess.ExecuteDataset("MTS_GETIMAPDETAILS", (object[])null);
        }
        public DataSet GetSTMPDetails()
        {
            return DataAccess.ExecuteDataset("MTS_GetSTMPDetails", (object[])null);
        }
        public object GetTimeSheetDetails(string currDate, int employeeId)
        {
            return DataAccess.ExecuteScalar("MTS_GETTIMESHEETDETAILS", new object[2]
            {
            (object) currDate,
            (object) employeeId
            });
        }
        public DataSet GetMTSIndiaEmployees()
        {
            return DataAccess.ExecuteDataset("MTS_GETMTSINDIAEMPLOYEES", (object[])null);
        }

        public object GetTemplateIDFromSchedule(long ScheduleID)
        {
            return DataAccess.ExecuteScalar("MTS_GETTEMPLATEIDFROMSCHEDULEID", new object[1]
            {
        (object) ScheduleID
            });
        }

        public DataSet GetEmailDataFromSP(string SPName, object[] Parameters)
        {
            return DataAccess.ExecuteDataset(SPName, Parameters);
        }
    }
}
