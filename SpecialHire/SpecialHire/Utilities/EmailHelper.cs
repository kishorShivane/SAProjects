using SpecialHire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace SpecialHire.Utilities
{
    public class EmailHelper
    {
        public static void SendQuotationConfirmation(BookingQuoteInfoModal bookingQuoteInfoModal)
        {
            try
            {
                //MailAddress mailfrom = new MailAddress("kishor.shv@gmail.com");
                //MailAddress mailto = new MailAddress("kishor.shv@gmail.com");
                //MailMessage newmsg = new MailMessage(mailfrom, mailto);

                //newmsg.Subject = "Subject of Email";
                //newmsg.Body = "Body(message) of email";

                ////For File Attachment, more file can also be attached
                //Attachment att = new Attachment(HttpContext.Current.Server.MapPath("~/PDF/"+bookingQuoteInfoModal.QuotationFileName));
                //newmsg.Attachments.Add(att);

                //SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                //smtp.UseDefaultCredentials = false;
                //smtp.Credentials = new NetworkCredential("kishor.shv@gmail.com", "kishkishor");
                //smtp.EnableSsl = true;
                //smtp.Send(newmsg);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}