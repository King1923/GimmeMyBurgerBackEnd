using System;
using System.Net;
using System.Net.Mail;

namespace LearningAPI.Helpers
{
    public class EmailService
    {
 

        public void SendResetEmail(string toEmail, string resetCode)
        {
            // Hard-coded SMTP configuration values.
            string fromMail = "nyptwentyonethreads@gmail.com";
            string fromPassword = "cwna lqsu oloj kkpn";

            // Create a new MailMessage.
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = "Password Reset Code"; // Use '=' instead of '-'
            message.To.Add(new MailAddress(toEmail));
            message.Body = $"<html><body>Your GMB OTP is: <strong>{resetCode}</strong>. The code is only valid for 3 minutes.</body></html>";
            message.IsBodyHtml = true;

            // Create and configure the SMTP client.
            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            })
            {
                try
                {
                    smtpClient.Send(message);
                }
                catch (Exception ex)
                {
                    // You can log the exception or rethrow it as needed.
                    throw new Exception("Error in EmailService.SendResetEmail", ex);
                }
            }
        }

        public void Send2faEmail(string toEmail, string resetCode)
        {
            // Hard-coded SMTP configuration values.
            string fromMail = "nyptwentyonethreads@gmail.com";
            string fromPassword = "cwna lqsu oloj kkpn";

            // Create a new MailMessage.
            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = "2FA Verification Code"; // Use '=' instead of '-'
            message.To.Add(new MailAddress(toEmail));
            message.Body = $"<html><body>Your GMB OTP is: <strong>{resetCode}</strong>. The code is only valid for 3 minutes.</body></html>";
            message.IsBodyHtml = true;

            // Create and configure the SMTP client.
            using (SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            })
            {
                try
                {
                    smtpClient.Send(message);
                }
                catch (Exception ex)
                {
                    // You can log the exception or rethrow it as needed.
                    throw new Exception("Error in EmailService.SendResetEmail", ex);
                }
            }
        }
    }
}
