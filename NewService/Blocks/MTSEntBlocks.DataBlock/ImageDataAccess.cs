using Microsoft.Practices.EnterpriseLibrary.Data;
using MTSEntBlocks.ExceptionBlock.Handlers;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;

namespace MTSEntBlocks.DataBlock
{
    /// <summary>
    /// </summary>
    public static class ImageDataAccess
    {
        private static readonly string _ConnectionName;
        private static readonly string _CommandTimeOut;

        static ImageDataAccess()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());

            if (string.IsNullOrEmpty(_ConnectionName))
            {
                _ConnectionName = ConfigurationManager.AppSettings["ImageDBConnectionName"];
            }

            if (string.IsNullOrEmpty(_CommandTimeOut))
            {
                _CommandTimeOut = ConfigurationManager.AppSettings["CommandTimeOut"];
            }
        }

        /// <summary>
        ///   Enterprise connection string name from configuration
        /// </summary>
        /// <param name="ConnectionName"></param>
        private static Database GetDatabase()
        {
            try
            {
                return DatabaseFactory.CreateDatabase(_ConnectionName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        ///   Gets the data table with image.
        /// </summary>
        /// <param name="ImageDocId">The image doc id for input of Store procedure.</param>
        /// <returns>data table with image</returns>
        public static DataTable GetImage(Int64 ImageDocId)
        {
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, "CUS_GETIMAGE", ImageDocId);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   This is private helper method
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns>DbCommand</returns>
        private static DbCommand GetCmd(Database db, string SPName, params object[] parameterValues)
        {
            try
            {
                DbCommand cmd = null;
                if (parameterValues == null)
                {
                    cmd = db.GetStoredProcCommand(SPName);
                }
                else
                {
                    cmd = db.GetStoredProcCommand(SPName, parameterValues);
                }

                cmd.CommandTimeout = Convert.ToInt32(string.IsNullOrEmpty(_CommandTimeOut) ? "30" : _CommandTimeOut);
                return cmd;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Gets the image.
        /// </summary>
        /// <param name="imagedocid">The image doc id for input of Store procedure.</param>
        /// <returns>
        ///   data reader with image
        /// </returns>
        public static IDataReader GetImages(Int64 imagedocid)
        {
            try
            {
                Database db = GetDatabase();
                return db.ExecuteReader(GetCmd(db, "CUS_GETIMAGE", imagedocid));
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Puts the image.
        /// </summary>
        /// <param name="frontimage">The front image.</param>
        /// <param name="rearimage">The rear image.</param>
        /// <returns></returns>
        public static Int64 PutImage(byte[] frontimage, byte[] rearimage)
        {
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = null;
                cmd = GetCmd(db, "CUS_PUTIMAGE", frontimage, rearimage);
                db.ExecuteNonQuery(cmd);
                return Convert.ToInt64(cmd.Parameters["@RETURN_VALUE"].Value);
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -999;
            }
        }

        /// <summary>
        ///   Executes the non query for delete the image - input parameter is image doc id
        /// </summary>
        /// <param name="imagedocid">The image doc id.</param>
        /// <returns></returns>
        public static void DeleteImage(Int64 imagedocid)
        {
            try
            {
                Database db = GetDatabase();
                db.ExecuteNonQuery(GetCmd(db, "CUS_DELETEIMAGE", imagedocid));
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
        }


        /*  ICL SPECIFICATION  */
        /// <summary>
        ///   Gets the data table with image.
        /// </summary>
        /// <param name="ImageDocId">The image doc id for input of Store procedure.</param>
        /// <returns>data table with image</returns>
        public static DataTable GetImageDocID(Int64 ImageDocId)
        {
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, "CUS_GETICLIMAGEVALIDATIONFAILEDIMAGEDOCID", ImageDocId);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                return dt;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;

            }
        }


        public static int Checkimageexist(Int64 imagedocid)
        {
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = null;
                cmd = GetCmd(db, "CUS_CHECKIMAGEEXIST", imagedocid);
                db.ExecuteNonQuery(cmd);
                return Convert.ToInt32(cmd.Parameters["@RETURN_VALUE"].Value);
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -999;
            }
        }
    }
}