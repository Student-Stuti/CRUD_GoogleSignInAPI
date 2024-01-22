using BAL.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static IdentityServer4.Models.IdentityResources;

namespace BAL.Services
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer = "smtp.Gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUsername = "mvctest17@gmail.com";
        private readonly string _smtpPassword = "bqhyobxynxwbkteo";
        public async Task SendForgotPasswordEmailAsync(string email, string callbackUrl)
        {
            using (var smtpClient = new SmtpClient(_smtpServer))
            {
                smtpClient.Port = _smtpPort;
                smtpClient.Credentials = new NetworkCredential("mvctest17@gmail.com"," bqhyobxynxwbkteo");
                smtpClient.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername),
                    Subject = "Password Reset",
                    Body = $"Click the following link to reset your password: {callbackUrl}",
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(email)
    ;

                await smtpClient.SendMailAsync(mailMessage);
            }
        
    }
    }
}
