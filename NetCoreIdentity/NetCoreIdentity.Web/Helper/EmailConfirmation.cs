using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NetCoreIdentity.Web.Helper
{
    public static class EmailConfirmation
    {
        public static void SendEmail(string link, string email)
        {
            SmtpClient sc = new SmtpClient();
            sc.Port = 587;
            sc.Host = "smtp.gmail.com";
            sc.EnableSsl = true;
            sc.UseDefaultCredentials = false;
            sc.Credentials = new NetworkCredential("akmustafa2511@gmail.com", "mustafa11mustafa");
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("eposta@gmail.com", "Deneme");
            mail.To.Add(email);
            mail.Subject = "E-Posta Doğrulama";
            mail.IsBodyHtml = true;
            mail.Body = $"Email adresini doğrulamak için aşağıdaki linke tıklayınız.<hr/><a href='{link}'>Şifre yenileme link</a>";
            mail.IsBodyHtml = true;
            sc.Send(mail);
        }
    }
}
