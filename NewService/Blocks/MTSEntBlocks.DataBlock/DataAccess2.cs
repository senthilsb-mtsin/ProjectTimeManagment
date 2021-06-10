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
    public sealed class DataAccess2
    {
        #region Private Variables

        private static string _ConnectionName;
        private static string _CommandTimeOut;

        #endregion

        #region Constructor

        static DataAccess2()
        {
            DatabaseFactory.ClearDatabaseProviderFactory();
            DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());
        }

        public DataAccess2(string ConnectionName)
        {
            _ConnectionName = ConfigurationManager.AppSettings[ConnectionName];

            if (string.IsNullOrEmpty(_CommandTimeOut))
            {
                _CommandTimeOut = ConfigurationManager.AppSettings["CommandTimeOut"];
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Enterprise connection string name from configuration
        /// </summary>
        /// <param name="ConnectionName"></param>
        private Database GetDatabase(string ConnectionName = null)
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
        ///   This method is used in GetCmdParameters method, send the command from GetCmdParameters
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        //private int ExecuteNonQuery(Database db, DbCommand cmd)
        //{
        //    try
        //    {
        //        return db.ExecuteNonQuery(cmd);
        //    }
        //    catch (Exception ex)
        //    {
        //        MTSExceptionHandler.HandleException(ref ex);

        //        return -999;
        //    }
        //}

        /// <summary>
        ///   This method gets all stored procedure parameter and fill in the Command as out parameter
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <returns>DbCommand</returns>
        //private DbCommand GetCmdParameters(string SPName)
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
        private DbCommand GetCmd(Database db, string SPName, params object[] parameterValues)
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

        #region Public Methods

        /// <summary>
        ///   Bulks the load.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dt">Data table with values.</param>
        /// <param name="colMapping">The column mapping with insert table.</param>
        public void BulkLoad(string tableName, DataTable dt, Dictionary<string, string> colMapping)
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

        /// <summary>
        ///   Gets the data table.
        /// </summary>
        /// <param name="spName">Name of the store procedure.</param>
        /// <param name="parameters">The parameters of store procedure</param>
        /// <returns></returns>
        public DataTable GetDataTable(string SPName, params object[] parameterValues)
        {
            try
            {
                DataTraceLogger.Log("Method Start GetDataTable", SPName, parameterValues);
                Database db = GetDatabase();
                DbCommand cmd = GetCmd(db, SPName, parameterValues);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTable dt = ds.Tables[0];
                DataTraceLogger.Log("Method Finish GetDataTable");
                return dt;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        /// <summary>
        ///   Executes the reader.
        /// </summary>
        /// <param name="SPName">Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string SPName, params object[] parameterValues)
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
        public DataSet ExecuteDataset(string SPName, params object[] parameterValues)
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
        ///   Executes the command text.
        /// </summary>
        /// <param name="commandText">The command text.</param>
        /// <returns></returns>
        public IDataReader ExecuteCommandText(string commandText)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteCommandText", commandText);
                Database db = GetDatabase();
                IDataReader _reader = db.ExecuteReader(CommandType.Text, commandText);
                DataTraceLogger.Log("Method Finish ExecuteCommandText");
                return _reader;
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
        public object ExecuteScalar(string SPName, params object[] parameterValues)
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
        public int ExecuteNonQuery(string SPName, params object[] parameterValues)
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
        /// </summary>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public DataTable ExecuteDataTable(string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteDataTable", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
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
        public DataSet ExecuteDataSet(string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteDataSet", sql);
                Database db = GetDatabase();
                DbCommand cmd = db.GetSqlStringCommand(sql);
                DataSet ds = db.ExecuteDataSet(cmd);
                DataTraceLogger.Log("Method Finish ExecuteDataSet");
                return ds;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);

                return null;
            }
        }

        /// <summary>
        ///   This method used where output parameters are required.
        /// </summary>
        /// <param name="SPName"> Name of the SP</param>
        /// <param name="parameterValues">parameter values in order of creation</param>
        /// <returns></returns>
        public DbCommand ExecuteNonQueryCMD(string SPName, params object[] parameterValues)
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
        /// </summary>
        /// <param name="SPName"></param>
        /// <param name="recordcount"></param>
        /// <param name="parameterValues"></param>
        /// <returns></returns>
        public IDataReader ExecuteReader(string SPName, out Int64 recordcount, params object[] parameterValues)
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
                DataTraceLogger.Log("Method Finish DataTraceLogger");
                return IdataReader;
            }
            catch (Exception ex)
            {
                MTSExceptionHandler.HandleException(ref ex);
                return null;
            }
        }

        #endregion
    }
}