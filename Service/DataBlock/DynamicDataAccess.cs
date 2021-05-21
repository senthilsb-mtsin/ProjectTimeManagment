using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTSEntBlocks.DataBlock
{
    public static class DynamicDataAccess
    {
        #region Private Methods

        /// <summary>
        /// Send Connection String to Connect DataBase
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
        /// 
        /// </summary>
        /// <param name="ConnectionString">DataBase Connection String</param>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataTable ExecuteSQLDataTable(string ConnectionString, string sql)
        {
            try
            {
                new TraceLogger().Log("Method Start", sql);
                Database db = GetSQLDatabase(ConnectionString);
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
        /// <param name="ConnectionString">DataBase Connection String</param>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static int ExecuteSQLNonQuery(string ConnectionString, string sql)
        {
            int result = 0;
            try
            {
                new TraceLogger().Log("Method Start", sql);
                Database db = GetSQLDatabase(ConnectionString);
                DbCommand cmd = db.GetSqlStringCommand(sql);
                result = db.ExecuteNonQuery(cmd);
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
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConnectionString">DataBase Connection String</param>
        /// <param name="sql">Full Query String</param>
        /// <returns></returns>
        public static DataSet ExecuteSQLDataSet(string ConnectionString, string sql)
        {
            try
            {
                new TraceLogger().Log("Method Start", sql);
                Database db = GetSQLDatabase(ConnectionString);
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

        #endregion
    }
}
