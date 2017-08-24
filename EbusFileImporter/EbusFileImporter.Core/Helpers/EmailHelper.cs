using EbusFileImporter.Logger;
using Newtonsoft.Json;
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
            string body = "";
            try
            {
                MailAddress mailfrom = null;
                SmtpClient smtp = null;

                using (StreamReader reader = new StreamReader(Constants.EmailTemplate + "EmailTemplate.html"))
                {
                    body = reader.ReadToEnd();
                }

                smtp = new SmtpClient(Constants.Host, Convert.ToInt32(Constants.Port));
                smtp.Credentials = new NetworkCredential(Constants.EmailUserName, Constants.EbusPassword);
                smtp.EnableSsl = false;
                mailfrom = new MailAddress(Constants.FromEmail);

                MailAddress mailto = new MailAddress(Constants.ToEmail);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);

                var message = GetMessageByEmailType(type);
                string file = Path.GetFileName(fileName);

                body = body.Replace("[*FileName*]", file);
                body = body.Replace("[*ClientName*]", customer);
                if (type == EmailType.Error)
                {
                    body = body.Replace("[*Error*]", "Error Message: <div style='color:red'>" + exception + "</div>");
                    newmsg.Subject = Constants.ErrorEmailSubject;
                }
                else
                {
                    body = body.Replace("[*Error*]", "");
                    newmsg.Subject = Constants.DuplicateEmailSubject;
                }
                body = body.Replace("[*Message*]", message);

                newmsg.IsBodyHtml = true;
                newmsg.Body = body;

                smtp.Send(newmsg);

                //System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                //var emailTolist = Constants.ToEmail;
                //foreach (var id in emailTolist.Split(';'))
                //{
                //    mail.To.Add(id);
                //}

                //mail.IsBodyHtml = true;
                //using (StreamReader reader = new StreamReader(Constants.EmailTemplate + "EmailTemplate.html"))
                //{
                //    body = reader.ReadToEnd();
                //}

                //var message = GetMessageByEmailType(type);
                //string file = Path.GetFileName(fileName);

                //body = body.Replace("[*FileName*]", file);
                //body = body.Replace("[*ClientName*]", customer);
                //if (type == EmailType.Error)
                //{
                //    body = body.Replace("[*Error*]", "Error Message: <div style='color:red'>" + exception + "</div>");
                //    mail.Subject = Constants.ErrorEmailSubject;
                //}
                //else
                //{
                //    body = body.Replace("[*Error*]", "");
                //    mail.Subject = Constants.DuplicateEmailSubject;
                //}
                //body = body.Replace("[*Message*]", message);

                //mail.From = new MailAddress(Constants.FromEmail);
                //mail.Body = body;
                //mail.IsBodyHtml = true;

                //SmtpClient client = new SmtpClient();
                //client.Credentials = new NetworkCredential(Constants.EmailUserName, Constants.EbusPassword);
                //client.UseDefaultCredentials = true;
                //client.Host = Constants.Host;
                //client.Port = Constants.Port;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;

                ////Send the msg
                //client.Send(mail);
            }
            catch (Exception ex)
            {
                Log.Error("Email Sending failed");
                Log.Info("Email Body: " + body);
                Log.Error("Exception - " + JsonConvert.SerializeObject(ex));               
                return;
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
                case EmailType.DateProblem:
                    result = "The following error occurred while importing the audit file and moved to date problem Folder.";
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
        Default,
        DateProblem
    }
}
