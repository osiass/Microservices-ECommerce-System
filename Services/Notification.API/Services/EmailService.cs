using MailKit.Net.Smtp;
using MimeKit;

namespace Notification.API.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendOrderConfirmationAsync(string toEmail, string userName, int orderId, decimal totalPrice)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("E-Ticaret", "noreply@eticaret.com"));
        message.To.Add(new MailboxAddress(userName, toEmail));
        message.Subject = $"Siparişiniz Alındı #{orderId}";

        message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;">
                    <h2 style="color:#000;">Siparişiniz için teşekkürler, {userName}!</h2>
                    <p>Sipariş numaranız: <strong>#{orderId}</strong></p>
                    <p>Toplam tutar: <strong>₺{totalPrice:N2}</strong></p>
                    <p>Siparişiniz en kısa sürede hazırlanıp kargoya verilecektir.</p>
                    <hr/>
                    <p style="color:#888;font-size:12px;">Bu mail otomatik olarak gönderilmiştir.</p>
                </div>
                """
        };

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync("sandbox.smtp.mailtrap.io", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("2b12a7f90aae9c", "cde182c9790e32");
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("[Notification] Mail gönderildi: {Email}, Sipariş #{OrderId}", toEmail, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Notification] Mail gönderilemedi: {Email}", toEmail);
        }
    }
}
