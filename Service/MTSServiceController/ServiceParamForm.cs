using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace ServiceController
{
    public partial class ServiceParamForm : Form
    {
        public static string RemoveAllNamespaces(string xmlDocument)
        {
            XElement xmlDocumentWithoutNs = RemoveAllNamespaces(XElement.Parse(xmlDocument));
            return xmlDocumentWithoutNs.ToString();
        }
        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }
            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        private DataSet dsServiceParams = new DataSet();
        private string _xmlData;
        
        public String XmlData {
            get
            {
                StringBuilder strXml = new StringBuilder();
                dsServiceParams.WriteXml(XmlWriter.Create(new StringWriter(strXml)));
                _xmlData = RemoveAllNamespaces(strXml.ToString());
                return _xmlData;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    XmlReader xmlread = XmlReader.Create(new StringReader(value));

                    dsServiceParams.ReadXml(xmlread);
                }
            }
        }
        public DataSet ServiceParam
        {
            get
            {
                return dsServiceParams;
            }
            set
            {
                dsServiceParams = value;
            }
        }

        public ServiceParamForm()
        {
            InitializeComponent();
        }

        private void ServiceParamForm_Load(object sender, EventArgs e)
        {
            try
            {
                if (dsServiceParams.Tables.Count > 0)
                {
                    dgserviceParam.DataSource = dsServiceParams.Tables[0];
                    dgserviceParam.Columns[0].ReadOnly = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            ServiceParam = dsServiceParams;
            if (ServiceParam.Tables.Count > 0)
            {
                this.DialogResult = DialogResult.Yes;
                this.Close();
            }
            else
            {
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }        
    }
}
