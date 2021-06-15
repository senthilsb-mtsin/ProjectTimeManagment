using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Configuration;

using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;

namespace MTSEntBlocks.DataBlock
{
    /// <summary>
    ///  This class performs database functions.
    /// </summary>
    public static class DataAccess
    {
        #region private varaibles

        private static string _ConnectionName;
        private static string _CommandTimeOut;

        #endregion

        #region constructor

        static DataAccess()
        {
            if (string.IsNullOrEmpty(_ConnectionName))
            {
                _ConnectionName = ConfigurationManager.AppSettings["AppConnectionName"];
            }
            if (string.IsNullOrEmpty(_CommandTimeOut))
            {
                _CommandTimeOut = ConfigurationManager.AppSettings["CommandTimeOut"];
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Enterprise connection string name from configuration
        /// </summary>
        /// <param name="ConnectionName"></param>
        private static Database GetDatabase(string ConnectionName = null)
        {
            /* Prakash : Changed for the Exception "Database provider factory not set for the static DatabaseFactory. Set a provider factory invoking the DatabaseFactory.SetProviderFactory method or by specifying custom mappings by calling the DatabaseFactory.SetDatabases method" */

            try
            {
                Database db = null;
                if (ConnectionName == null)
                {
                    return db = new DatabaseProviderFactory().Create(_ConnectionName);
                    //return db = DatabaseFactory.CreateDatabase(_ConnectionName);
                }
                else
                {
                    return db = new DatabaseProviderFactory().Create(ConnectionName);
                    //return db = DatabaseFactory.CreateDatabase(ConnectionName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This method is used in GetCmdParameters method, send the command from GetCmdParameters  
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static int ExecuteNonQuery(Database db, DbCommand cmd)
        {
            try
            {
                return db.ExecuteNonQuery(cmd);
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return -999;
            }
        }

        /// <summary>
        /// This method gets all stored procedure parameter and fill in the Command as out parameter 
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <returns>DbCommand</returns>
        private static DbCommand GetCmdParameters(string SPName)
        {
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName);
                db.DiscoverParameters(cmd);
                return cmd;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }

        }

        /// <summary>
        /// This is private helper method
        /// </summary>
        /// <param name="SPName"> Name of the Store procedure</param>
        /// <param name="parameterValues">parameter values for input of store procedure</param>
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

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }

        }
        
        #endregion

        #region public methods

        /// <summary>
        /// Executes the reader.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                IDataReader result = db.ExecuteReader(GetCmd(db, SPName, parameterValues));
                new TraceLogger().Log("Method Finish");
                return result;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Executes the dataset.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static DataSet ExecuteDataset(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                DataSet result = db.ExecuteDataSet(GetCmd(db, SPName, parameterValues));
                new TraceLogger().Log("Method Finish");
                return result;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Executes the scalar.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static object ExecuteScalar(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                object result = db.ExecuteScalar(GetCmd(db, SPName, parameterValues));
                new TraceLogger().Log("Method Finish");
                return result;
            }
            catch (Exception ex)
            {


                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Executes the non query.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                int result = db.ExecuteNonQuery(GetCmd(db, SPName, parameterValues));
                new TraceLogger().Log("Method Finish");
                return result;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return -999;
            }
        }

        /// <summary>
        /// This method used where output parameters are required.
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static DbCommand ExecuteNonQueryCMD(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                DbCommand cmd = null;
                cmd = GetCmd(db, SPName, parameterValues);
                db.ExecuteNonQuery(cmd);
                new TraceLogger().Log("Method Finish");
                return cmd;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Returns IDataReader for Bigger Datasets
        /// </summary>
        /// <param name="SPName"></param>
        /// <param name="recordcount"></param>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string SPName, out Int64 recordcount, params object[] parameterValues)
        {
            new TraceLogger().Log("Method Start", SPName, parameterValues);
            recordcount = 0;
            IDataReader IdataReader;
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName, parameterValues);
                IdataReader = db.ExecuteReader(cmd);
                recordcount = Convert.ToInt64(cmd.Parameters["@Return_Value"].Value);
                new TraceLogger().Log("Method Finish");
                return IdataReader;
            }
            catch (Exception ex)
            {
                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the data table.
        /// </summary>
        /// <param name="spName">Name of the store procedure.</param>
        /// <param name="parameterValues">The parameters of store procedure</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string SPName, params object[] parameterValues)
        {
            try
            {
                new TraceLogger().Log("Method Start", SPName, parameterValues);
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName, parameterValues);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                new TraceLogger().Log("Method Finish");
                return dt;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataTable ExecuteSQLDataTable(string sql)
        {
            try
            {
                new TraceLogger().Log("Method Start", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                new TraceLogger().Log("Method Finish");
                return dt;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataSet ExecuteSQLDataSet(string sql)
        {
            try
            {
                new TraceLogger().Log("Method Start", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
                DataSet ds = db.ExecuteDataSet(cmd);
                new TraceLogger().Log("Method Finish");
                return ds;
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return null;
            }
        }

        /// <summary>
        /// Bulks the load.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dt">Data table with values.</param>
        /// <param name="colMapping">The column mapping with insert table.</param>
        public static void bulkLoad(string tableName, DataTable dt, Dictionary<string, string> colMapping)
        {
            new TraceLogger().Log("Method Start", tableName);
            DbConnection con = GetDatabase().CreateConnection();
            try
            {
                SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)con, SqlBulkCopyOptions.KeepIdentity, null);
                foreach (var item in colMapping)
                {
                    bulkCopy.ColumnMappings.Add(item.Key, item.Value);
                }

                bulkCopy.DestinationTableName = tableName;
                bulkCopy.BulkCopyTimeout = Convert.ToInt32(string.IsNullOrEmpty(_CommandTimeOut) ? "30" : _CommandTimeOut);

                con.Open();
                bulkCopy.WriteToServer(dt);
                new TraceLogger().Log("Method Finish");
            }
            catch (Exception ex)
            {

                bool rethrow = DataAccessExceptionHandler.HandleException(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
            }
            finally
            {
                con.Close();
            }
        }

        #endregion
    }
}