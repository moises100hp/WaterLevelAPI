using System.Net;
using System.Net.Mail;

namespace WaterLevelAPI.Service
{
    public static class EmailService
    {
        public static async Task SendPasswordResetEmailAsync(string email, string temporaryPassword)
        {
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? "smtp.gmail.com";
            var smtpPortText = Environment.GetEnvironmentVariable("SMTP_PORT") ?? "587";
            var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
            var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var fromEmail = Environment.GetEnvironmentVariable("SMTP_FROM_EMAIL") ?? smtpUser ?? "noreply@waterlevel.local";

            if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                throw new InvalidOperationException("SMTP não está configurado para recuperação de senha.");
            }

            if (!int.TryParse(smtpPortText, out var smtpPort))
                throw new InvalidOperationException("SMTP_PORT inválida.");

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUser, smtpPassword)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = "Sua senha temporária da WaterLevel",
                Body = $"Sua senha temporária é: {temporaryPassword}\n\nRecomendamos alterar sua senha após o login.",
                IsBodyHtml = false
            };

            message.To.Add(email);

            await client.SendMailAsync(message);
        }
    }
}
