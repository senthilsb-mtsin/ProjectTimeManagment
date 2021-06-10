using MTS.ServiceBase;
using MTSEntBlocks.DataBlock;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.LoggerBlock;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ImportToEphesoft
{
    public class ImportToEphesoft : IMTSServiceBase
    {
        //private static string EphesoftInputPath = string.Empty;
        private string InputFolderPath;
        private string UNCNetworkPath;
        public bool DoTask()
        {
            try
            {
                //CreateDIPFile(InboundPath);
                Logger.WriteTraceLog($"Calling Method DoTask()");
                ProcessFiles();
                Logger.WriteTraceLog($"Called Method DoTask()");


            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
            return true;
        }

        public void OnStart(string ServiceParam)
        {
            Logger.WriteTraceLog($"Calling Method OnStart()");
            var Params = XDocument.Parse(ServiceParam).Descendants("add").Select(z => new { Key = z.Attribute("key").Value, Value = z.Value }).ToList();
            InputFolderPath = Params.Find(f => f.Key == "ShareDrivePath").Value;
            Logger.WriteTraceLog($"InputFolderPath: { InputFolderPath}");
            Logger.WriteTraceLog($"Called Method OnStart()");

        }

        private bool ProcessFiles()
        {
            Logger.WriteTraceLog($"Enters ProcessFiles function");
            string[] Files = Directory.GetFiles(InputFolderPath, "*" + ".pdf", SearchOption.AllDirectories);

            Logger.WriteTraceLog($"Found  { Files.Length.ToString()} files in {InputFolderPath}");

            if (Files.Length == 0)
                return true;

            foreach (string file in Files)
            {
                Logger.WriteTraceLog($"Processing File: {file}");
                string fileNameWithExtension = Path.GetFileName(file);

                string pdfName = Path.GetFileNameWithoutExtension(file);
                Logger.WriteTraceLog($"pdfName: {pdfName}");
                string documentType = string.Empty;
                int configId = 0;
                Int64 mappingId = 0;

                string currentDirectory = Path.GetDirectoryName(@file);
                Logger.WriteTraceLog($"currentDirectory: {currentDirectory}");

                string fullPathOnly = Path.GetFullPath(currentDirectory);
                Logger.WriteTraceLog($"fullPathOnly: {fullPathOnly}");
                string dropFolderPath = string.Empty;
                DataTable mappingData = GetMappingDetailByURL(fullPathOnly);
                if (mappingData.Rows.Count > 0)
                {
                    Logger.WriteTraceLog($"mapping found for the path: {fullPathOnly}");

                    documentType = Convert.ToString(mappingData.Rows[0]["DocumentType"]);
                    Logger.WriteTraceLog($"documentType: { documentType} ");
                    configId = Convert.ToInt32(mappingData.Rows[0]["Id"]);
                    Logger.WriteTraceLog($"configId: { configId} ");
                    string bcId = Convert.ToString(mappingData.Rows[0]["BCIdentifier"]);
                    Logger.WriteTraceLog($"BCIdentifier: {bcId } ");

                    DataTable bcConfig = GetEphesoftInputPath(Convert.ToString(mappingData.Rows[0]["BCIdentifier"]));
                    if (bcConfig.Rows.Count > 0)
                        UNCNetworkPath = Convert.ToString(bcConfig.Rows[0]["EphesoftInputPath"]);

                    Logger.WriteTraceLog($"Ephesoft Drop path for {bcId} is: { UNCNetworkPath} ");

                    mappingId = GetMappingId();
                    string batchName = $"{pdfName}_{documentType}_{Convert.ToInt32(mappingId)}";
                    Logger.WriteTraceLog($"batchName is: {batchName}");

                    Int64 lastId = AddLDIMapping(batchName, configId);
                    Logger.WriteTraceLog($"mapping added successfully and the reference id is: {lastId}");
                    
                    dropFolderPath = UNCNetworkPath;
                    dropFolderPath = Path.Combine(UNCNetworkPath, batchName);
                    Logger.WriteTraceLog($"UNCNetworkPath is: {dropFolderPath}");

                    if (!Directory.Exists(dropFolderPath))
                        Directory.CreateDirectory(dropFolderPath);

                    File.Move(file, Path.Combine(dropFolderPath, fileNameWithExtension));
                    Logger.WriteTraceLog($"File Droped in ephesoft input folder successfully...!");

                }
                else
                {
                    Logger.WriteTraceLog($"No mapping available in the LDI Configuration table for the path: {fullPathOnly}");
                }
                
            }

            return true;
        }
        private Int64 AddLDIMapping(string batchName, int mappingId)
        {
            return Convert.ToInt64(DataAccess.ExecuteScalar("CREATE_LDI_MAPPING", batchName, mappingId));
        }

        public static Int16 GetMappingId()
        {
            DbCommand cmd = DataAccess.ExecuteNonQueryCMD("GET_LDI_MappingID");
            return Convert.ToInt16(cmd.Parameters["@RETURN_VALUE"].Value.ToString());

        }

        public static DataTable GetMappingDetailByURL(string importPath)
        {
            return DataAccess.ExecuteDataTable("GET_MAPPING_DETAIL_BYURL", importPath);
        }

        public static DataTable GetEphesoftInputPath(string bcIdentifier)
        {
            return DataAccess.ExecuteDataTable("GET_DROPPATH_BYID", bcIdentifier);
        }
    }
}
