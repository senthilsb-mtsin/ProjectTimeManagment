using MTSEntBlocks.DataBlock;
using System;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace ServiceController
{
    public partial class DBQuery : Form
    {   
        int sRowsaffected = 0;        
        string sConfrmMsg = string.Empty;
        public DBQuery()
        {
            InitializeComponent();
            txtQuery.Focus();
        }
        private void btnExecute_Click(object sender, EventArgs e)
        {               
            GetValues();  
        }

        private void AssignScriptandBindGrid(string sGetTableName, string sQry, string strValue)
        {
            if (sGetTableName != null)
            {
                sQry = strValue;
                BindGrid(sQry, sGetTableName);
            }
            else
                sQry = strValue;  
        }

        private DataTable ExecuteScript(string Query, string sTableName)
        {
            try
            {
                DataTable dtEmpty=new DataTable();
                DbCommand cmd = DataAccess.ExecuteNonQueryCMD("MTS_DBQUERYEXECUTESCRIPT", Query);
                sRowsaffected = Convert.ToInt16(cmd.Parameters["@RETURN_VALUE"].Value.ToString());
                if (Query.Contains("INSERT") || Query.Contains("UPDATE") || Query.Contains("DELETE"))
                {
                    DataTable dt = DataAccess.ExecuteDataTable("MTS_DBQUERYGETDATA", sTableName);
                    return dt;
                }
                else
                {
                    DataTable dt = DataAccess.ExecuteDataTable("MTS_DBQUERYGETDATACOLUMNS", Query);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }       
        private void GetValues()
        {
            try
            {
                sConfrmMsg = string.Empty;
                string sQry = string.Empty;
                string strValue = string.Empty;
                strValue = txtQuery.Text.ToString();                            
                string sGetTableName = string.Empty;

                if (strValue != string.Empty)
                {
                    sGetTableName = GetTableName(strValue);
                    if (sGetTableName != string.Empty)
                    {
                        if (strValue.Contains("UPDATE"))                       
                            sConfrmMsg = "Update";                        
                        else if (strValue.Contains("INSERT"))
                            sConfrmMsg = "Insert";                        
                        else if (strValue.Contains("DELETE"))
                            sConfrmMsg = "Delete";

                        if (sConfrmMsg != string.Empty)
                        {
                            // If Query contains Insert, Update and Delete means confirmation dialog box shows
                            if (MessageBox.Show("Do you want to  execute  this query?", "" + sConfrmMsg + "", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
                                AssignScriptandBindGrid(sGetTableName, sQry, strValue);                            
                        }                        
                        else if (strValue.Contains("SELECT"))
                            // If Query contains Select means Bind the Grid
                            AssignScriptandBindGrid(sGetTableName, sQry, strValue);                        
                        else 
                            BindGrid(strValue, sGetTableName);
                    }
                    else                   
                        BindGrid(strValue, sGetTableName);                    
                }
                else
                {
                    txtQuery.Focus();
                    dgResult.DataSource = null;
                    lblRowsCount.Text = string.Empty;
                    lblRowsaffected.Text = string.Empty;                    
                }
            }
            catch (Exception ex)
            {   
                string[] split = ex.Message.Split('.');
                lblError.Text = "Following Error Occured :\r\n" + split[0];
                lblRowsCount.Text = string.Empty;
                lblRowsaffected.Text = string.Empty;
                dgResult.DataSource = null;
            }
        }
        public void BindGrid(string Qry, string Table)
        {
            DataTable dataTable = ExecuteScript(Qry, Table);
            dgResult.DataSource = dataTable.DefaultView;
            AutoNumberRowsForGridView(dgResult);
            lblRowsCount.Text = dgResult.RowCount.ToString() + " rows";
            if (sConfrmMsg != string.Empty)
                lblRowsaffected.Text = "(" + sRowsaffected + " row(s) affected)";
            else
                lblRowsaffected.Text = string.Empty;
            lblError.Text = string.Empty;                        
        }
        private string GetTableName(string Query)
        {
            string sTabName = string.Empty;
            string line = Query.ToString();
            Regex r = new Regex(@"(from|update|Into)\s+(?<table>\S+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);            
            Match m = r.Match(line);            
            string[] sInsertTabSplit = m.Groups["table"].Value.Split('(');            
            sTabName = sInsertTabSplit[0];                
            return sTabName;
        }
        private void AutoNumberRowsForGridView(DataGridView dataGridView)
        {
            if (dataGridView != null)
            {                
                for (int count = 0; (count <= (dataGridView.RowCount - 1)); count++)                
                {
                    dataGridView.Rows[count].HeaderCell.Value = string.Format((count + 1).ToString(), "0");
                }
            }
        }
        private void txtQuery_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                GetValues();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to close the DBQuery?", "DBQuery", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.OK)
            {
                this.Close();
            }  
        }
      
    }
}
