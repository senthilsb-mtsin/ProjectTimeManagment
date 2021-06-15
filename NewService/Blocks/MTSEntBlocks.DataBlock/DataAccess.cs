using Microsoft.Practices.EnterpriseLibrary.Data;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.LoggerBlock;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace MTSEntBlocks.DataBlock
{
    /// <summary>
    ///   This class performs database functions.
    /// </summary>
    public static class DataAccess
    {
        #region private varaibles

        private static readonly string _ConnectionName;
        private static readonly string _CommandTimeOut;

        #endregion

        #region Constructor

        static DataAccess()
        {

            DatabaseFactory.ClearDatabaseProviderFactory();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());

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
        ///   Enterprise connection string name from configuration
        /// </summary>
        /// <param name="ConnectionName"></param>
        private static Database GetDatabase(string ConnectionName = null)
        {

            try
            {
                Database db = null;
                if (ConnectionName == null)
                {
                    return db = DatabaseFactory.CreateDatabase(_ConnectionName);
                }
                else
                {
                    return db = DatabaseFactory.CreateDatabase(ConnectionName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        ///   This method gets all stored procedure parameter and fill in the Command as out parameter
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <returns>DbCommand</returns>
        //private static DbCommand GetCmdParameters(string SPName)
        //{
        //    try
        //    {
        //        Database db = GetDatabase();
        //        DbCommand cmd = GetCmd(db, SPName);
        //        db.DiscoverParameters(cmd);
        //        return cmd;
        //    }
        //    catch (Exception ex)
        //    {
        //        MTSExceptionHandler.HandleException(ref ex);
        //        return null;
        //    }
        //}

        /// <summary>
        ///   This is private helper method
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
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        ///   Executes the reader.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string SPName, params object[] parameterValues)
        {
            try
            {

                DataTraceLogger.Log("Method Start ExecuteReader", SPName, parameterValues);
                Database db = GetDatabase();
                IDataReader result = db.ExecuteReader(GetCmd(db, SPName, parameterValues));
                DataTraceLogger.Log("Method Finish ExecuteReader");
                return result;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Executes the dataset.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static DataSet ExecuteDataset(string SPName, params object[] parameterValues)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteDataset", SPName, parameterValues);
                Database db = GetDatabase();
                DataSet result = db.ExecuteDataSet(GetCmd(db, SPName, parameterValues));
                DataTraceLogger.Log("Method Finish ExecuteDataset");
                return result;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Executes the scalar.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static object ExecuteScalar(string SPName, params object[] parameterValues)
        {
            try
            {

                DataTraceLogger.Log("Method Start ExecuteScalar", SPName, parameterValues);
                Database db = GetDatabase();
                object result = db.ExecuteScalar(GetCmd(db, SPName, parameterValues));
                DataTraceLogger.Log("Method Finish ExecuteScalar");
                return result;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Executes the non query.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string SPName, params object[] parameterValues)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteNonQuery", SPName, parameterValues);
                Database db = GetDatabase();
                int result = db.ExecuteNonQuery(GetCmd(db, SPName, parameterValues));
                DataTraceLogger.Log("Method Finish ExecuteNonQuery");
                return result;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return -999;
            }
        }

        /// <summary>
        ///   This method used where output parameters are required.
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public static DbCommand ExecuteNonQueryCMD(string SPName, params object[] parameterValues)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteNonQueryCMD", SPName, parameterValues);
                Database db = GetDatabase();
                DbCommand cmd = null;
                cmd = GetCmd(db, SPName, parameterValues);
                db.ExecuteNonQuery(cmd);
                DataTraceLogger.Log("Method Finish ExecuteNonQueryCMD");
                return cmd;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Returns IDataReader for Bigger Datasets
        /// </summary>
        /// <param name="SPName"></param>
        /// <param name="recordcount"></param>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        public static IDataReader ExecuteReader(string SPName, out Int64 recordcount, params object[] parameterValues)
        {
            DataTraceLogger.Log("Method Start ExecuteReader", SPName, parameterValues);
            recordcount = 0;
            IDataReader IdataReader;
            try
            {
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName, parameterValues);
                IdataReader = db.ExecuteReader(cmd);
                recordcount = Convert.ToInt64(cmd.Parameters["@Return_Value"].Value);
                DataTraceLogger.Log("Method Finish ExecuteReader");
                return IdataReader;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Gets the data table.
        /// </summary>
        /// <param name="spName">Name of the store procedure.</param>
        /// <param name="parameterValues">The parameters of store procedure</param>
        /// <returns></returns>
        public static DataTable ExecuteDataTable(string SPName, params object[] parameterValues)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteDataTable", SPName, parameterValues);
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName, parameterValues);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                DataTraceLogger.Log("Method Finish ExecuteDataTable");
                return dt;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataTable ExecuteSQLDataTable(string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteSQLDataTable", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                DataTraceLogger.Log("Method Finish ExecuteSQLDataTable");
                return dt;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataSet ExecuteSQLDataSet(string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteSQLDataSet", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTraceLogger.Log("Method Finish ExecuteSQLDataSet");
                return ds;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Bulks the load.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dt">Data table with values.</param>
        /// <param name="colMapping">The column mapping with insert table.</param>
        public static void BulkLoad(string tableName, DataTable dt, Dictionary<string, string> colMapping)
        {
            DataTraceLogger.Log("Method Start bulkLoad", tableName);
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
                DataTraceLogger.Log("Method Finish bulkLoad");
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
            }
            finally
            {
                con.Close();
            }
        }

        #endregion
    }
}



