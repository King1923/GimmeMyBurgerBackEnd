using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LearningAPI.Helpers
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendResetEmail(string toEmail, string resetCode)
        {
            // Read configuration values.
            string smtpServer = _configuration["Smtp:Server"];
            int smtpPort = int.Parse(_configuration["Smtp:Port"]);
            string smtpEmail = _configuration["Smtp:Email"];
            string smtpPassword = _configuration["Smtp:Password"];

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpEmail, smtpPassword),
                EnableSsl = true,
            };

            smtpClient.Send(smtpEmail, toEmail, "Password Reset Code", $"Your password reset code is: {resetCode}");
        }
    }
}
