using MTS.ServiceBase;
using System.Linq;
using System.Xml.Linq;

namespace MTS.EmailEmpStaging
{
    public class EmailEmpStaging : IMTSServiceBase
    {
        private string EmailEmpStagingSPName;
        private string Dayofweek;

        public void OnStart(string ServiceParams)
        {
            var list = XDocument.Parse(ServiceParams).Descendants((XName)"add").Select(z => new
            {
                Key = z.Attribute((XName)"key").Value,
                Value = z.Value
            }).ToList();
            this.EmailEmpStagingSPName = list.Find(f => f.Key == "EmailEmpStagingSPName").Value;
            this.Dayofweek = list.Find(f => f.Key == "Day").Value;
        }

        public bool DoTask()
        {
            return this.DoEmailStaging();
        }

        private bool DoEmailStaging()
        {
            if (string.IsNullOrEmpty(this.EmailEmpStagingSPName))
                return false;
            EmailEmpStagingDataAccess.DoEmailStaging(this.EmailEmpStagingSPName, this.Dayofweek);
            return true;
        }
    }
}
