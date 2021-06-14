using MTS.ServiceBase;
using MTSEntBlocks.DataBlock;
using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace ServiceDebugger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            object objService = null;
            Type objtype = null;
            DataTable dtServiceInfo = new DataTable();
            try
            {
                dtServiceInfo = DataAccess.ExecuteDataTable("[dbo].[MTS_GETSERVICECONFIGFORSERVICE]", "TEST_MTSTREX_ProjectCreator"); //serviceList.Text);
                string DLLName = dtServiceInfo.Rows[0]["DLLName"].ToString();
                string ServiceParams = dtServiceInfo.Rows[0]["ServiceParams"].ToString();
                Assembly ServiceDll = Assembly.LoadFile(Environment.CurrentDirectory + @"\" + DLLName);

                foreach (Type type in ServiceDll.GetTypes())
                {
                    if (typeof(IMTSServiceBase).IsAssignableFrom(type))
                    {
                        objtype = type;
                        break;
                    }
                }
                objService = Activator.CreateInstance(objtype);
                objtype.InvokeMember("OnStart", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, objService, new object[] { ServiceParams });
                objtype.InvokeMember("DoTask", BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public, null, objService, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void serviceList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
