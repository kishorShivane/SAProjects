using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mail;

namespace Reports.Web.Helpers
{
    public static class MailHelper
    {
        public static bool SendMailToEbus(string cardID, string reason, string comments, string userName, string companyName)
        {
            var result = true;
            var isFromGmail = ConfigurationManager.AppSettings["UseGmailForEmail"] == null ? false : ConfigurationManager.AppSettings["UseGmailForEmail"] == "true" ? true : false;
            var emailTolist = ConfigurationManager.AppSettings.Get("EmailToList");

            var message = "Hi" + "\r\n";
            message += "\r\n";
            message += string.Format("{0} from {1} requesting a card to be hot-listed.", userName, companyName);
            message += Environment.NewLine;
            message += string.Format("Smart Card Serial Number :- {0}", cardID);
            message += "\r\n";
            message += string.Format("Reason :- {0}", reason);
            message += "\r\n";
            message += string.Format("Comments :- {0}", comments);
            message += "\r\n";
            message += "Card Control File Entry:" + MasterHelper.GenerateLCRCode(Convert.ToDecimal(cardID));
            message += "\r\n";
            message += "\r\n";
            message += "Regards,";
            message += "\r\n";
            message += "\r\n";
            message += "eBus Supplies";

            var email = new System.Net.Mail.MailMessage
            {
                Body = message,
                Subject = "Smart Card Hot List Requested : " + cardID + " From : " + companyName,
                IsBodyHtml = false
            }; 

            foreach (var id in emailTolist.Split(';'))
            {
                email.To.Add(id);
            }

            email.BodyEncoding = UTF8Encoding.UTF8;
            email.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            SmtpClient client = new SmtpClient();
            if (!isFromGmail)
            {
                email.From = new MailAddress(ConfigurationManager.AppSettings["FromEmail"]);
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["UserName"], ConfigurationManager.AppSettings["Password"]);
                client.Host = ConfigurationManager.AppSettings["SMTP"];
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPort"]); ;
            }
            else
            {
                email.From = new MailAddress(ConfigurationManager.AppSettings["GmailFromEmail"]);
                client.Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["GmailUserName"], ConfigurationManager.AppSettings["GmailPassword"]);
                client.Host = ConfigurationManager.AppSettings["GmailSMTP"];
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["GmailSMTPPort"]);
                client.EnableSsl = true;
            }
            
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            try
            {
                client.Send(email);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }

        public static bool SendServiceRequestMail(string filepath, string email, string requestType, string serviceRequestID, string user, string comments, string priority, string requestStatus)
        {
            bool result = true;
            string body = "";
            try
            {
                MailAddress mailfrom = null;
                SmtpClient smtp = null;

                using (StreamReader reader = new StreamReader(filepath))
                {
                    body = reader.ReadToEnd();
                }

                smtp = new SmtpClient("mail.ebussupplies.co.za", Convert.ToInt32(25));
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Credentials = new NetworkCredential("info@ebussupplies.co.za", "ebus0117836833");
                mailfrom = new MailAddress("info@ebussupplies.com");

                MailAddress mailto = new MailAddress(email);
                System.Net.Mail.MailMessage newmsg = new System.Net.Mail.MailMessage(mailfrom, mailto);

                var subject = "eBus Supplies Customer Service Request ##ServiceRequestID## has been ##Status##";

                if (requestStatus == "Closed")
                {
                    subject.Replace("##ServiceRequestID##", serviceRequestID).Replace("##Status##", "closed");
                }
                else if (requestStatus == "Created")
                {
                    subject.Replace("##ServiceRequestID##", serviceRequestID).Replace("##Status##", "raised");
                }

                body = body.Replace("##RequestType##", requestType);
                body = body.Replace("##ServiceRequestID##", serviceRequestID);
                body = body.Replace("##User##", user);
                body = body.Replace("##Comments##", comments);
                body = body.Replace("##Priority##", priority);
                newmsg.IsBodyHtml = true;
                newmsg.Body = body;
                newmsg.Subject = subject;
                smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                result = false;
                return result;
            }
            return result;
        }
    }
}