using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace MTS.EmailManager
{
    internal class EmailTemplate
    {
        public Template CreateEmailTemplateAndProcess(string htmlpage, string queryStr)
        {
            XmlDocument TemplateAndData = new XmlDocument();
            TemplateAndData.LoadXml(this.GetEmailBody(htmlpage, queryStr));
            return this.FillEmailTemplate(TemplateAndData);
        }

        private string GetEmailBody(string page, string queryStr)
        {
            return new StreamReader(WebRequest.Create(ConfigurationManager.AppSettings["EmailTemplateBaseUrl"] + page + "?ID=" + HttpUtility.UrlEncode(queryStr)).GetResponse().GetResponseStream(), Encoding.UTF8).ReadToEnd();
        }

        private Template FillEmailTemplate(XmlDocument TemplateAndData)
        {
            Template template = new Template();
            template.From = TemplateAndData.SelectSingleNode("email/From").InnerText != "" ? TemplateAndData.SelectSingleNode("email/From").InnerText : template.From;
            template.To = TemplateAndData.SelectSingleNode("email/To").InnerText != "" ? TemplateAndData.SelectSingleNode("email/To").InnerText : template.To;
            template.Cc = TemplateAndData.SelectSingleNode("email/Cc").InnerText != "" ? TemplateAndData.SelectSingleNode("email/Cc").InnerText : template.Cc;
            template.BCc = TemplateAndData.SelectSingleNode("email/BCc").InnerText != "" ? TemplateAndData.SelectSingleNode("email/BCc").InnerText : template.BCc;
            template.Subject = TemplateAndData.SelectSingleNode("email/Subject").InnerText != "" ? TemplateAndData.SelectSingleNode("email/Subject").InnerText : template.Subject;
            template.Body = TemplateAndData.SelectSingleNode("email/Body").InnerText != "" ? TemplateAndData.SelectSingleNode("email/Body").InnerText : template.Body;
            return template;
        }
    }
}
