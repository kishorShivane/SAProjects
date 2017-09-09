using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;

namespace EBusReportsService
{
    public class EmailHelper
    {
        public static void SendEmail(string customer, List<string> attachmentFilePaths)
        {
            try
            {
                MailAddress mailfrom = null;
                SmtpClient smtp = null;
                string customerName = Helper.GetCustomerDisplayName(customer);

                if (Constants.Gmail == "true")
                {
                    smtp = new SmtpClient(Constants.GmailSMTP, Convert.ToInt32(Constants.GmailSMTPPort));
                    smtp.Credentials = new NetworkCredential(Constants.GmailUserName, Constants.GmailPassword);
                    smtp.EnableSsl = true;
                    mailfrom = new MailAddress(Constants.GmailUserName);
                }
                else
                {
                    smtp = new SmtpClient(Constants.SMTP, Convert.ToInt32(Constants.SMTPPort));
                    smtp.Credentials = new NetworkCredential(Constants.UserName, Constants.Password);
                    smtp.EnableSsl = false;
                    smtp.UseDefaultCredentials = false;
                    mailfrom = new MailAddress(Constants.UserName);
                }
                               

                //MailAddress mailto = new MailAddress(Constants.ToEmail);
                MailMessage newmsg = new MailMessage();

                newmsg.From = mailfrom;

                var toMailAddress = ConfigurationSettings.AppSettings[customer.Trim() + "ToEmail"].Split(';');
                Helper.WriteToFile("Service Info: {0} " + ConfigurationSettings.AppSettings[customer + "ToEmail"] + " To Email");
                foreach (var email in toMailAddress)
                {
                    if (!string.IsNullOrEmpty(email))
                        newmsg.To.Add(email);
                }


                newmsg.Subject = Constants.EmailSubject.Replace("##CUSTOMER##", customerName);

                newmsg.IsBodyHtml = true;
                var mailBody = GetEmailBody();
                newmsg.Body = mailBody.Replace("##CUSTOMER##", customerName);
                Attachment att = null;
                foreach (var file in attachmentFilePaths)
                {
                    if (!File.Exists(file))
                    {
                        Helper.WriteToFile("Service Info: {0} " + file + " file exist check in progress");
                        Thread.Sleep(100);
                    }
                    Helper.WriteToFile("Service Info: {0} " + file + " file exist check passed");
                    Helper.WriteToFile("Service Info: {0} " + file + " attached");
                    //For File Attachment, more file can also be attached
                    att = new Attachment(file);
                    newmsg.Attachments.Add(att);
                }


                smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                Helper.WriteToFile("Service Error on: {0} " + ex.Message + ex.StackTrace);
                throw;
            }
        }

       

        public static string GetEmailBody()
        {
            string body = "<html><body style='color:black; font-size:15px;'><font face='Helvetica, Arial, sans-serif'><div><p>Dear ##CUSTOMER## User,</p><p style='color: red; '>This is an automated email sent from eBus Wayfarer Back Office.</p><p>Please find attached the following reports you have subscribed to receive daily:</p><ul><li>Communication Status for Today</li><li>Scheduled But Not Operated for yesterday</li></ul><p>For any queries regarding this email or its content please reply to this email for our support staff to get back to you.</p><p>To unsubscribe: reply with the word 'UNSUBSCRIBE' in the SUBJECT line.</p><br>Best Regards,<br>eBus Supplies<br>Tel: +27 (0)11 476 5400<br>Fax: +27 (0)86 554 2482<br>email: support@ebussupplies.co.za <br>website: www.ebussupplies.co.za <br>ADDRESS: 33 Judges Avenue, Randburg, 2194 <br>POSTAL: P.O. Box 139, Douglasdale, 2165 <br><br>DISCLAIMER:<br>This message and any attachments are confidential and intended solely for the addressee. If you have received this message in error, please notify eBus Supplies immediately, on telephone number: +27 (0)11 - 476 5400 or e-mail: info@ebussupplies.co.za <br>Any unauthorised use, alteration or dissemination is prohibited. eBus Supplies accepts no liability whatsoever for any loss, whether it be direct, indirect or consequential, arising from information made available and actions resulting there from.<br>The views or representations contained in this message, whether express or implied, are those of the sender only, unless that sender expressly states them to be the views or representations of an entity or person, who shall be named by the sender and who the sender shall state to represent shall otherwise attach to any other entity or person. </div></body></html>";
            return body;
        }
    }
}
