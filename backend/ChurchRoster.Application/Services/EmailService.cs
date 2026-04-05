using ChurchRoster.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ChurchRoster.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly string _appUrl;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _smtpHost = _configuration["EmailSettings:SmtpServer"] ?? "smtp-relay.brevo.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:Username"] ?? "";
            _smtpPassword = _configuration["EmailSettings:Password"] ?? "";
            _fromEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@churchroster.com";
            _fromName = _configuration["EmailSettings:SenderName"] ?? "Church Ministry Roster";
            _appUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:5173";

            // Log configuration on initialization (without sensitive data)
            _logger.LogInformation("EmailService initialized with:");
            _logger.LogInformation("  SMTP Host: {SmtpHost}", _smtpHost);
            _logger.LogInformation("  SMTP Port: {SmtpPort}", _smtpPort);
            _logger.LogInformation("  From Email: {FromEmail}", _fromEmail);
            _logger.LogInformation("  From Name: {FromName}", _fromName);
            _logger.LogInformation("  App URL: {AppUrl}", _appUrl);
            _logger.LogInformation("  Username Configured: {UsernameConfigured}", !string.IsNullOrEmpty(_smtpUsername));
            _logger.LogInformation("  Password Configured: {PasswordConfigured}", !string.IsNullOrEmpty(_smtpPassword));

            // Warn if credentials are missing
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogWarning("⚠️ SMTP credentials are not configured! Email sending will fail.");
                _logger.LogWarning("Please configure EmailSettings:Username and EmailSettings:Password in appsettings.json");
            }
        }

        public async Task<bool> SendInvitationEmailAsync(string toEmail, string toName, string invitationToken, string invitedByName)
        {
            _logger.LogInformation("=== Starting SendInvitationEmailAsync ===");
            _logger.LogInformation("Recipient: {ToEmail}, Name: {ToName}", toEmail, toName);
            _logger.LogInformation("Invited by: {InvitedByName}", invitedByName);
            _logger.LogInformation("Token: {Token}", invitationToken.Substring(0, Math.Min(10, invitationToken.Length)) + "...");

            try
            {
                var acceptUrl = $"{_appUrl}/accept-invitation?token={invitationToken}";
                _logger.LogInformation("Accept URL: {AcceptUrl}", acceptUrl);

                var subject = "You're Invited to Join Church Ministry Roster";
                var body = GetInvitationEmailTemplate(toName, invitedByName, acceptUrl);

                var result = await SendEmailAsync(toEmail, toName, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Invitation email sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("❌ Failed to send invitation email to {ToEmail}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in SendInvitationEmailAsync for {ToEmail}: {Message}", toEmail, ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return false;
            }
        }

        public async Task<bool> SendAssignmentNotificationAsync(string toEmail, string toName, string taskName, DateTime eventDate)
        {
            _logger.LogInformation("=== Starting SendAssignmentNotificationAsync ===");
            _logger.LogInformation("Recipient: {ToEmail}, Task: {TaskName}, Date: {EventDate}", toEmail, taskName, eventDate);

            try
            {
                var subject = $"New Ministry Assignment: {taskName}";
                var body = GetAssignmentNotificationTemplate(toName, taskName, eventDate);

                var result = await SendEmailAsync(toEmail, toName, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Assignment notification sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("❌ Failed to send assignment notification to {ToEmail}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in SendAssignmentNotificationAsync: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
        {
            _logger.LogInformation("=== Starting SendPasswordResetEmailAsync ===");
            _logger.LogInformation("Recipient: {ToEmail}", toEmail);

            try
            {
                var resetUrl = $"{_appUrl}/reset-password?token={resetToken}";

                var subject = "Reset Your Password - Church Ministry Roster";
                var body = GetPasswordResetTemplate(toName, resetUrl);

                var result = await SendEmailAsync(toEmail, toName, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Password reset email sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("❌ Failed to send password reset email to {ToEmail}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in SendPasswordResetEmailAsync: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendAssignmentReminderAsync(string toEmail, string toName, string taskName, DateTime eventDate, int daysUntil)
        {
            _logger.LogInformation("=== Starting SendAssignmentReminderAsync ===");
            _logger.LogInformation("Recipient: {ToEmail}, Task: {TaskName}, Date: {EventDate}, Days: {Days}", toEmail, taskName, eventDate, daysUntil);

            try
            {
                var subject = $"Reminder: {taskName} - {daysUntil} days away";
                var body = GetAssignmentReminderTemplate(toName, taskName, eventDate, daysUntil);

                var result = await SendEmailAsync(toEmail, toName, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Assignment reminder sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("❌ Failed to send assignment reminder to {ToEmail}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in SendAssignmentReminderAsync: {Message}", ex.Message);
                return false;
            }
        }

        public async Task<bool> SendAssignmentRevokedNotificationAsync(string toEmail, string toName, string taskName, DateTime eventDate, string reason)
        {
            _logger.LogInformation("=== Starting SendAssignmentRevokedNotificationAsync ===");
            _logger.LogInformation("Recipient: {ToEmail}, Task: {TaskName}, Date: {EventDate}", toEmail, taskName, eventDate);

            try
            {
                var subject = $"Assignment Cancelled: {taskName}";
                var body = GetAssignmentRevokedTemplate(toName, taskName, eventDate, reason);

                var result = await SendEmailAsync(toEmail, toName, subject, body);

                if (result)
                {
                    _logger.LogInformation("✅ Assignment revoked notification sent successfully to {ToEmail}", toEmail);
                }
                else
                {
                    _logger.LogError("❌ Failed to send assignment revoked notification to {ToEmail}", toEmail);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ EXCEPTION in SendAssignmentRevokedNotificationAsync: {Message}", ex.Message);
                return false;
            }
        }

        private async Task<bool> SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            _logger.LogInformation(">>> SendEmailAsync called");
            _logger.LogInformation("  To: {ToEmail} ({ToName})", toEmail, toName);
            _logger.LogInformation("  Subject: {Subject}", subject);
            _logger.LogInformation("  Body length: {BodyLength} characters", htmlBody.Length);

            try
            {
                // Validate configuration
                if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
                {
                    _logger.LogError("❌ SMTP credentials are not configured!");
                    _logger.LogError("  Username configured: {UsernameConfigured}", !string.IsNullOrEmpty(_smtpUsername));
                    _logger.LogError("  Password configured: {PasswordConfigured}", !string.IsNullOrEmpty(_smtpPassword));
                    throw new InvalidOperationException("SMTP credentials are not configured. Please check appsettings.json");
                }

                _logger.LogInformation("Creating email message...");
                using var message = new MailMessage();
                message.From = new MailAddress(_fromEmail, _fromName);
                message.To.Add(new MailAddress(toEmail, toName));
                message.Subject = subject;
                message.Body = htmlBody;
                message.IsBodyHtml = true;

                _logger.LogInformation("Email message created successfully");
                _logger.LogInformation("  From: {FromEmail} ({FromName})", _fromEmail, _fromName);
                _logger.LogInformation("  To: {ToEmail} ({ToName})", toEmail, toName);

                _logger.LogInformation("Creating SMTP client...");
                _logger.LogInformation("  Host: {SmtpHost}", _smtpHost);
                _logger.LogInformation("  Port: {SmtpPort}", _smtpPort);
                _logger.LogInformation("  Username: {Username}", _smtpUsername);
                _logger.LogInformation("  SSL Enabled: true");

                using var smtpClient = new SmtpClient(_smtpHost, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                smtpClient.EnableSsl = true;
                smtpClient.Timeout = 30000; // 30 seconds timeout

                _logger.LogInformation("Attempting to send email via SMTP...");
                await smtpClient.SendMailAsync(message);

                _logger.LogInformation("✅✅✅ EMAIL SENT SUCCESSFULLY to {ToEmail} ✅✅✅", toEmail);
                return true;
            }
            catch (SmtpFailedRecipientException ex)
            {
                _logger.LogError(ex, "❌ SMTP Failed Recipient Exception:");
                _logger.LogError("  Failed Recipient: {FailedRecipient}", ex.FailedRecipient);
                _logger.LogError("  Status Code: {StatusCode}", ex.StatusCode);
                _logger.LogError("  Message: {Message}", ex.Message);
                return false;
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "❌ SMTP Exception:");
                _logger.LogError("  Status Code: {StatusCode}", ex.StatusCode);
                _logger.LogError("  Message: {Message}", ex.Message);
                _logger.LogError("  Inner Exception: {InnerException}", ex.InnerException?.Message);
                _logger.LogError("Check your SMTP credentials and server settings!");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ UNEXPECTED EXCEPTION in SendEmailAsync:");
                _logger.LogError("  Exception Type: {ExceptionType}", ex.GetType().Name);
                _logger.LogError("  Message: {Message}", ex.Message);
                _logger.LogError("  Stack Trace: {StackTrace}", ex.StackTrace);
                return false;
            }
        }

        private string GetInvitationEmailTemplate(string toName, string invitedByName, string acceptUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Invitation to Church Ministry Roster</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #4F46E5 0%, #7C3AED 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 28px;"">🎉 You're Invited!</h1>
    </div>

    <div style=""background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 18px; color: #4F46E5; font-weight: bold;"">Hello {toName},</p>

        <p style=""font-size: 16px;"">
            <strong>{invitedByName}</strong> has invited you to join the <strong>Church Ministry Roster System</strong>.
        </p>

        <p style=""font-size: 16px;"">
            This system helps manage ministry assignments, track availability, and coordinate our church's volunteer teams.
        </p>

        <div style=""background: #F3F4F6; padding: 20px; border-radius: 8px; margin: 25px 0;"">
            <h3 style=""margin-top: 0; color: #1F2937;"">What's Next?</h3>
            <ol style=""margin: 0; padding-left: 20px;"">
                <li style=""margin-bottom: 10px;"">Click the button below to accept your invitation</li>
                <li style=""margin-bottom: 10px;"">Create your secure password</li>
                <li style=""margin-bottom: 10px;"">Start managing your ministry assignments</li>
            </ol>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{acceptUrl}"" style=""background: #4F46E5; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block;"">
                Accept Invitation
            </a>
        </div>

        <p style=""font-size: 14px; color: #6B7280; border-top: 1px solid #E5E7EB; padding-top: 20px; margin-top: 30px;"">
            <strong>Note:</strong> This invitation link will expire in 7 days. If you didn't expect this invitation or have any questions, please contact your church administrator.
        </p>

        <p style=""font-size: 12px; color: #9CA3AF; margin-top: 20px; text-align: center;"">
            Church Ministry Roster System<br>
            This is an automated message, please do not reply to this email.
        </p>
    </div>
</body>
</html>";
        }

        private string GetAssignmentNotificationTemplate(string toName, string taskName, DateTime eventDate)
        {
            var formattedDate = eventDate.ToString("dddd, MMMM d, yyyy");

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>New Ministry Assignment</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #10B981 0%, #059669 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 28px;"">📋 New Assignment</h1>
    </div>

    <div style=""background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 18px; color: #10B981; font-weight: bold;"">Hello {toName},</p>

        <p style=""font-size: 16px;"">
            You have been assigned to serve in the following ministry:
        </p>

        <div style=""background: #F3F4F6; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 4px solid #10B981;"">
            <p style=""margin: 0 0 10px 0; font-size: 14px; color: #6B7280;"">Ministry Task</p>
            <h2 style=""margin: 0 0 15px 0; color: #1F2937;"">{taskName}</h2>
            <p style=""margin: 0; font-size: 14px; color: #6B7280;"">Event Date</p>
            <p style=""margin: 5px 0 0 0; font-size: 18px; font-weight: bold; color: #4F46E5;"">{formattedDate}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{_appUrl}/my-assignments"" style=""background: #10B981; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block; margin-right: 10px;"">
                View Assignment
            </a>
        </div>

        <p style=""font-size: 14px; color: #6B7280; border-top: 1px solid #E5E7EB; padding-top: 20px; margin-top: 30px;"">
            Please log in to the Church Ministry Roster System to accept or decline this assignment.
        </p>

        <p style=""font-size: 12px; color: #9CA3AF; margin-top: 20px; text-align: center;"">
            Church Ministry Roster System<br>
            This is an automated message, please do not reply to this email.
        </p>
    </div>
</body>
</html>";
        }

        private string GetPasswordResetTemplate(string toName, string resetUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Reset Your Password</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #EF4444 0%, #DC2626 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 28px;"">🔐 Password Reset</h1>
    </div>

    <div style=""background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 18px; color: #EF4444; font-weight: bold;"">Hello {toName},</p>

        <p style=""font-size: 16px;"">
            We received a request to reset your password for the Church Ministry Roster System.
        </p>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{resetUrl}"" style=""background: #EF4444; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block;"">
                Reset Password
            </a>
        </div>

        <p style=""font-size: 14px; color: #6B7280;"">
            If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
        </p>

        <p style=""font-size: 14px; color: #6B7280; border-top: 1px solid #E5E7EB; padding-top: 20px; margin-top: 30px;"">
            <strong>Note:</strong> This reset link will expire in 1 hour for security reasons.
        </p>

        <p style=""font-size: 12px; color: #9CA3AF; margin-top: 20px; text-align: center;"">
            Church Ministry Roster System<br>
            This is an automated message, please do not reply to this email.
        </p>
    </div>
</body>
</html>";
        }

        private string GetAssignmentReminderTemplate(string toName, string taskName, DateTime eventDate, int daysUntil)
        {
            var formattedDate = eventDate.ToString("dddd, MMMM d, yyyy");
            var urgencyColor = daysUntil <= 7 ? "#EF4444" : "#F59E0B";
            var urgencyText = daysUntil <= 7 ? "Coming Up Soon!" : "Upcoming";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Ministry Assignment Reminder</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, {urgencyColor} 0%, #DC2626 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 28px;"">⏰ Reminder: {urgencyText}</h1>
    </div>

    <div style=""background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 18px; color: {urgencyColor}; font-weight: bold;"">Hello {toName},</p>

        <p style=""font-size: 16px;"">
            This is a friendly reminder about your upcoming ministry assignment:
        </p>

        <div style=""background: #FEF3C7; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 4px solid {urgencyColor}; text-align: center;"">
            <h2 style=""margin: 0 0 15px 0; color: #1F2937;"">{taskName}</h2>
            <p style=""margin: 0; font-size: 14px; color: #6B7280;"">Scheduled for</p>
            <p style=""margin: 5px 0 15px 0; font-size: 20px; font-weight: bold; color: #4F46E5;"">{formattedDate}</p>
            <div style=""background: {urgencyColor}; color: white; display: inline-block; padding: 10px 20px; border-radius: 20px; font-weight: bold; font-size: 18px;"">
                {daysUntil} {(daysUntil == 1 ? "day" : "days")} away
            </div>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{_appUrl}/my-assignments"" style=""background: #10B981; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block;"">
                View My Assignments
            </a>
        </div>

        <p style=""font-size: 14px; color: #6B7280; border-top: 1px solid #E5E7EB; padding-top: 20px; margin-top: 30px;"">
            Please ensure you're prepared for this assignment. If you have any questions or concerns, contact your ministry coordinator.
        </p>

        <p style=""font-size: 12px; color: #9CA3AF; margin-top: 20px; text-align: center;"">
            Church Ministry Roster System<br>
            This is an automated reminder message.
        </p>
    </div>
</body>
</html>";
        }

        private string GetAssignmentRevokedTemplate(string toName, string taskName, DateTime eventDate, string reason)
        {
            var formattedDate = eventDate.ToString("dddd, MMMM d, yyyy");
            var reasonText = string.IsNullOrEmpty(reason) ? "No specific reason provided." : reason;

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Assignment Cancelled</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background: linear-gradient(135deg, #6B7280 0%, #4B5563 100%); color: white; padding: 30px; border-radius: 10px 10px 0 0; text-align: center;"">
        <h1 style=""margin: 0; font-size: 28px;"">🔄 Assignment Cancelled</h1>
    </div>

    <div style=""background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-top: none; border-radius: 0 0 10px 10px;"">
        <p style=""font-size: 18px; color: #6B7280; font-weight: bold;"">Hello {toName},</p>

        <p style=""font-size: 16px;"">
            We're writing to inform you that the following ministry assignment has been cancelled:
        </p>

        <div style=""background: #F3F4F6; padding: 20px; border-radius: 8px; margin: 25px 0; border-left: 4px solid #6B7280;"">
            <p style=""margin: 0 0 10px 0; font-size: 14px; color: #6B7280;"">Ministry Task</p>
            <h2 style=""margin: 0 0 15px 0; color: #1F2937;"">{taskName}</h2>
            <p style=""margin: 0; font-size: 14px; color: #6B7280;"">Original Event Date</p>
            <p style=""margin: 5px 0 0 0; font-size: 18px; font-weight: bold; color: #4F46E5;"">{formattedDate}</p>
        </div>

        <div style=""background: #FEF3C7; padding: 15px; border-radius: 8px; margin: 25px 0; border-left: 4px solid #F59E0B;"">
            <p style=""margin: 0 0 5px 0; font-size: 12px; color: #92400E; font-weight: bold; text-transform: uppercase;"">Reason</p>
            <p style=""margin: 0; color: #78350F;"">{reasonText}</p>
        </div>

        <div style=""text-align: center; margin: 30px 0;"">
            <a href=""{_appUrl}/my-assignments"" style=""background: #6B7280; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold; display: inline-block;"">
                View My Assignments
            </a>
        </div>

        <p style=""font-size: 14px; color: #6B7280; border-top: 1px solid #E5E7EB; padding-top: 20px; margin-top: 30px;"">
            If you have any questions about this cancellation, please contact your ministry coordinator or church administrator.
        </p>

        <p style=""font-size: 12px; color: #9CA3AF; margin-top: 20px; text-align: center;"">
            Church Ministry Roster System<br>
            This is an automated notification message.
        </p>
    </div>
</body>
</html>";
        }
    }
}
