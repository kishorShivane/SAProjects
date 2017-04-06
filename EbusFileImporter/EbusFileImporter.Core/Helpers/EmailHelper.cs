using EbusFileImporter.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EbusFileImporter.Core.Helpers
{
    public class EmailHelper
    {
        private static ILogService Log;
        public EmailHelper(ILogService logger)
        {
            Log = logger;
        }

        public void SendMail(string fileName, string customer, string exception, EmailType type)
        {
            try
            {
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                var toMail = ConfigurationManager.AppSettings["ToMail"];
                mail.To.Add(toMail);

                mail.IsBodyHtml = true;
                string body = "";
                using (StreamReader reader = new StreamReader("C:\\eBusSuppliesTGX150AuditFiles\\EmailTemplate.html"))
                {
                    body = reader.ReadToEnd();
                }
                var message = GetMessageByEmailType(type);
                string file = Path.GetFileName(fileName);
                body = body.Replace("[*FileName*]", file);
                body = body.Replace("[*ClientName*]", customer);
                body = body.Replace("[*Error*]", exception);
                body = body.Replace("[*Message*]", message);
                MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                mail.From = address;
                mail.Subject = "Ebus Import Error";
                mail.Body = body;
                mail.IsBodyHtml = true;
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = false;
                client.Host = "mail.ebussupplies.co.za";
                client.Port = 25;

                //Setup credentials to login to our sender email address ("UserName", "Password")
                NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                client.UseDefaultCredentials = true;
                client.Credentials = credentials;

                //Send the msg
                client.Send(mail);
                Log.Info(message);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetMessageByEmailType(EmailType type)
        {
            var result = "";
            switch (type)
            {
                case EmailType.Error:
                    result = "The following error occurred while importing the audit file and moved to Error Folder.";
                    break;
                case EmailType.Duplicate:
                    result = "The following error occurred while importing the audit file and moved to Duplicate Folder.";
                    break;
                case EmailType.Default:
                    result = "Unknown error";
                    break;
            }
            return result;
        }
    }

    public enum EmailType
    {
        Error,
        Duplicate,
        Default
    }
}
