using System.Net;
using System.Net.Mail;
using ProductTrackingAPI.Data;
using ProductTrackingAPI.Entities;
using ProductTrackingAPI.Interface;

namespace ProductTrackingAPI.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public EmailService(
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task SendMismatchEmailAsync(
    long saleOrderId,
    string saleOrderNo,
    string stage,
    int expectedCount,
    int actualCount,
    List<string> missingHuNumbers)
    {
        var toEmails = _configuration
            .GetSection("EmailSettings:MismatchTo")
            .Get<List<string>>() ?? [];

        var ccEmails = _configuration
            .GetSection("EmailSettings:MismatchCc")
            .Get<List<string>>() ?? [];

        var bccEmails = _configuration
            .GetSection("EmailSettings:MismatchBcc")
            .Get<List<string>>() ?? [];

        var subject = $"🚨 Mismatch Alert - {saleOrderNo}";

        var body = $@"
<html>
<head>
<style>
body {{
    font-family: Arial, sans-serif;
    background-color: #f4f4f4;
    margin: 0;
    padding: 20px;
}}

.container {{
    max-width: 700px;
    margin: auto;
    background: #ffffff;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}}

.header {{
    background-color: #dc3545;
    color: white;
    padding: 20px;
    font-size: 22px;
    font-weight: bold;
}}

.content {{
    padding: 20px;
}}

.summary {{
    width: 100%;
    border-collapse: collapse;
    margin-top: 15px;
}}

.summary td {{
    border: 1px solid #dddddd;
    padding: 10px;
}}

.label {{
    background-color: #f8f9fa;
    font-weight: bold;
    width: 35%;
}}

.alert {{
    color: #dc3545;
    font-size: 18px;
    font-weight: bold;
}}

.footer {{
    background-color: #f8f9fa;
    padding: 15px;
    text-align: center;
    font-size: 12px;
    color: #666666;
}}

ul {{
    margin-top: 10px;
}}

li {{
    margin-bottom: 5px;
}}
</style>
</head>
<body>

<div class='container'>

    <div class='header'>
         Missing Package Alert
    </div>

    <div class='content'>

        <p class='alert'>
            Package count mismatch detected in Lift Tracking System.
        </p>

        <table class='summary'>
            <tr>
                <td class='label'>Sale Order</td>
                <td>{saleOrderNo}</td>
            </tr>
            <tr>
                <td class='label'>Stage</td>
                <td>{stage}</td>
            </tr>
            <tr>
                <td class='label'>Expected Count</td>
                <td>{expectedCount}</td>
            </tr>
            <tr>
                <td class='label'>Actual Count</td>
                <td>{actualCount}</td>
            </tr>
            <tr>
                <td class='label'>Missing Count</td>
                <td>{missingHuNumbers.Count}</td>
            </tr>
        </table>

        <h3>Missing HU Numbers</h3>

        <ul>
            {string.Join("", missingHuNumbers.Select(x => $"<li>{x}</li>"))}
        </ul>

    </div>

    <div class='footer'>
        This is an automated notification from the Lift Tracking System.<br/>
        Please do not reply to this email.
    </div>

</div>

</body>
</html>";

        try
        {
            using var client = new SmtpClient(
                _configuration["EmailSettings:Host"])
            {
                Port = int.Parse(
                    _configuration["EmailSettings:Port"]!),

                Credentials =
                    new NetworkCredential(
                        _configuration["EmailSettings:Username"],
                        _configuration["EmailSettings:Password"]),

                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(
                    _configuration["EmailSettings:FromEmail"]!),

                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            foreach (var email in toEmails)
            {
                mail.To.Add(email);
            }

            foreach (var email in ccEmails)
            {
                mail.CC.Add(email);
            }

            foreach (var email in bccEmails)
            {
                mail.Bcc.Add(email);
            }

            await client.SendMailAsync(mail);

            _context.EmailLogs.Add(
                new EmailLog
                {
                    SaleOrderId = saleOrderId,
                    Stage = stage,
                    Recipients =
                        $"TO:{string.Join(",", toEmails)} | " +
                        $"CC:{string.Join(",", ccEmails)} | " +
                        $"BCC:{string.Join(",", bccEmails)}",

                    Subject = subject,
                    Status = "SENT",
                    SentAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _context.EmailLogs.Add(
                new EmailLog
                {
                    SaleOrderId = saleOrderId,
                    Stage = stage,
                    Recipients =
                        $"TO:{string.Join(",", toEmails)} | " +
                        $"CC:{string.Join(",", ccEmails)} | " +
                        $"BCC:{string.Join(",", bccEmails)}",

                    Subject = subject,
                    Status = "FAILED",
                    ErrorMessage = ex.ToString(),
                    SentAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            throw;
        }
    }
}