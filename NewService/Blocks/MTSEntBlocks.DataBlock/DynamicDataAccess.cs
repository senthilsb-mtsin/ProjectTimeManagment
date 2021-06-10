using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using MTSEntBlocks.ExceptionBlock.Handlers;
using MTSEntBlocks.LoggerBlock;
using System;
using System.Data;
using System.Data.Common;

namespace MTSEntBlocks.DataBlock
{
    public static class DynamicDataAccess
    {
        #region Private Methods

        /// <summary>
        ///   Send Connection String to Connect DataBase
        /// </summary>
        /// <param name="ConnectionString"></param>
        private static Database GetSQLDatabase(string ConnectionString = "")
        {
            try
            {
                Database db = new SqlDatabase(ConnectionString);
                return db;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



        #region Public Methods

        /// <summary>
        /// </summary>
        /// <param name="ConnectionString">DataBase Connection String</param>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataTable ExecuteSQLDataTable(string ConnectionString, string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteSQLDataTable", sql);
                Database db = GetSQLDatabase(ConnectionString);
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
        /// <param name="ConnectionString">DataBase Connection String</param>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataSet ExecuteSQLDataSet(string ConnectionString, string sql)
        {
            try
            {
                DataTraceLogger.Log("Method Start ExecuteSQLDataSet", sql);
                Database db = GetSQLDatabase(ConnectionString);
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

        #endregion
    }
}