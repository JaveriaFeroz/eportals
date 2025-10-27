using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace ProcureToPay.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
        Task SendCredentialsEmailAsync(string email, string username, string temporaryPassword);
        Task SendConfirmationEmailAsync(string email, string confirmationLink);
    }
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; } = true;
    }
    public class OutlookEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<OutlookEmailService> _logger;

        public OutlookEmailService(IOptions<EmailSettings> emailSettings, ILogger<OutlookEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(email));

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;

                    await client.SendMailAsync(message);
                    _logger.LogInformation($"Email sent successfully to {email}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {email}");
                throw;
            }
        }

        public async Task SendCredentialsEmailAsync(string email, string username, string temporaryPassword)
        {
            string subject = "Your ePortal Account Credentials";

            string htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; }}
                        .container {{ padding: 20px; }}
                        .header {{ background-color: #4472C4; color: white; padding: 10px; }}
                        .content {{ padding: 20px; }}
                        .credentials {{ background-color: #f5f5f5; padding: 15px; margin: 15px 0; }}
                        .footer {{ font-size: 12px; color: #666; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>ePortal - Account Created</h2>
                        </div>
                        <div class='content'>
                            <p>Hello,</p>
                            <p>Your account has been created in the ePortal system. Please use the following credentials to log in:</p>
                            
                            <div class='credentials'>
                                <p><strong>Username:</strong> {username}</p>
                                <p><strong>Temporary Password:</strong> {temporaryPassword}</p>
                            </div>
                            
                            <p><strong>Important:</strong> You will be required to change your password upon first login.</p>
                            <p>If you have any questions, please contact your system administrator.</p>
                        </div>
                        <div class='footer'>
                            <p>This is an automated message. Please do not reply to this email.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }

        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            string subject = "Confirm your ePortal Account";

            string htmlBody = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px; }}
                    .container {{ background-color: #ffffff; padding: 20px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); max-width: 600px; margin: 0 auto; }}
                    .header {{ text-align: center; background-color: #092963; color: white; padding: 20px; border-radius: 8px 8px 0 0; }}
                    .content {{ padding: 20px 0; }}
                    .button-link {{ display: inline-block; padding: 10px 20px; font-size: 16px; color: white; background-color: #092963; text-decoration: none; border-radius: 5px; }}
                    .footer {{ text-align: center; font-size: 12px; color: #666; margin-top: 20px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Email Confirmation Required</h2>
                    </div>
                    <div class='content'>
                        <p>Hello,</p>
                        <p>Thank you for creating an account with ePortal. Please confirm your email address by clicking the link below:</p>
                        <p style='text-align: center;'>
                            <a href='{confirmationLink}' class='button-link'>Confirm Email</a>
                        </p>
                        <p>This confirmation is required to activate your account. Once confirmed, you will be able to log in.</p>
                        <p>If you did not create this account, please ignore this email.</p>
                    </div>
                    <div class='footer'>
                        <p>This is an automated message. Please do not reply.</p>
                    </div>
                </div>
            </body>
            </html>";

            await SendEmailAsync(email, subject, htmlBody);
        }
    }
}

