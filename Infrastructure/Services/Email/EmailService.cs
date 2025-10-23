using Application.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Infrastructure.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) ||
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
                {
                    _logger.LogWarning("Email configuration is incomplete. Email not sent.");
                    return false;
                }

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

                using var message = new MailMessage();
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(toEmail);
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;
                message.BodyEncoding = Encoding.UTF8;
                message.SubjectEncoding = Encoding.UTF8;

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent successfully to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {toEmail}");
                return false;
            }
        }

        public async Task<bool> SendMissionCreatedEmailAsync(string email, string createBy, string sendTo, string missionTitle, string deadline)
        {
            try
            {
                var subject = "Mission Assigned - Learn Link";
                var body = GenerateMissionCreatedEmailBody(sendTo, missionTitle, createBy, deadline);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                return false;
            }
        }

        public async Task<bool> SendMissionReviewedEmailAsync(string email, string createBy, string sendTo, string missionTitle, string status, string feedback, int? score)
        {
            try
            {
                var subject = "Mission Reviewed - Learn Link";
                var body = GenerateMissionReviewedEmailBody(sendTo, missionTitle, createBy, status,feedback, score);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                return false;
            }
        }

        public async Task<bool> SendMissionStartedEmailAsync(string email, string createBy, string sendTo, string missionTitle)
        {
            try
            {
                var subject = "Mission Started - Learn Link";
                var body = GenerateMissionStartedEmailBody(sendTo, missionTitle, createBy);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                return false;
            }
        }

        public async Task<bool> SendMissionSubmittedEmailAsync(string email, string createBy, string sendTo, string missionTitle)
        {
            try
            {
                var subject = "Mission Submitted - Learn Link";
                var body = GenerateMissionSubmittedEmailBody(sendTo, missionTitle, createBy);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
                return false;
            }
        }

        public Task<bool> SendOtpEmailAsync(string email, string otp, string userName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendPasswordResetConfirmationAsync(string email, string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                var subject = "Welcome to Learn Link! 🎉";
                var body = GenerateWelcomeEmailBody(userName);

                return await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                return false;
            }
        }

        private string GenerateMissionCreatedEmailBody(string sendTo, string missionTitle, string createdBy, string deadline)
        {
            var beUrl = _configuration["Settings:BeUrl"];
            var feUrl = _configuration["Settings:FeUrl"];
            var logoPath = _configuration["Settings:LogoPath"];
            var logoUrl = $"{beUrl}{logoPath}";
            var missionLink = $"{feUrl}/child/dashboard";
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>New Mission Created - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #3b82f6, #2563eb);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .mission-container {{
                        background: linear-gradient(135deg, #dbeafe, #bfdbfe);
                        padding: 25px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #3b82f6;
                    }}
                    .mission-title {{
                        font-size: 20px;
                        font-weight: bold;
                        color: #1e40af;
                        margin: 0 0 15px 0;
                    }}
                    .mission-details {{
                        background: white;
                        padding: 15px;
                        border-radius: 6px;
                        margin: 15px 0;
                    }}
                    .detail-row {{
                        display: flex;
                        justify-content: space-between;
                        padding: 8px 0;
                        border-bottom: 1px solid #e5e7eb;
                    }}
                    .detail-row:last-child {{
                        border-bottom: none;
                    }}
                    .detail-label {{
                        font-weight: 600;
                        color: #4b5563;
                    }}
                    .detail-value {{
                        color: #1e40af;
                    }}
                    .cta {{
                        background: linear-gradient(135deg, #3b82f6, #2563eb);
                        padding: 20px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: white;
                        color: #3b82f6;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 10px 0;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <img src='{logoUrl}' alt='LearnLink Logo' />
                        </div>
                        <h1>New Mission for You! 🎯</h1>
                    </div>
        
                    <p>Hi <strong>{sendTo}</strong>! 👋</p>
        
                    <p>Awesome news! <strong>{createdBy}</strong> has created a new mission just for you! This is your chance to learn something new and earn awesome points! 🌟</p>
        
                    <div class='mission-container'>
                        <div class='mission-title'>📋 {missionTitle}</div>
                        <div class='mission-details'>
                            <div class='detail-row'>
                                <span class='detail-label'>Created by:</span>
                                <span class='detail-value'>{createdBy}</span>
                            </div>
                            <div class='detail-row'>
                                <span class='detail-label'>Deadline:</span>
                                <span class='detail-value'>{deadline}</span>
                            </div>
                        </div>
                    </div>
        
                    <p>Ready to take on this challenge? Start whenever you're ready and show what you can do! Remember, every mission completed brings you closer to amazing rewards! 🏆</p>
        
                    <div class='cta'>
                        <p style='color: white; margin: 0 0 15px 0;'>Let's get started!</p>
                        <a href='{missionLink}' class='button'>View My Mission</a>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateMissionStartedEmailBody(string sendTo, string missionTitle, string createdBy)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Child Started Mission - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #f59e0b, #d97706);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .status-container {{
                        background: linear-gradient(135deg, #fef3c7, #fde68a);
                        padding: 25px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #f59e0b;
                        text-align: center;
                    }}
                    .status-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .mission-info {{
                        background: #f8fafc;
                        padding: 20px;
                        border-radius: 8px;
                        margin: 20px 0;
                        border: 1px solid #e2e8f0;
                    }}
                    .info-row {{
                        padding: 10px 0;
                        border-bottom: 1px solid #e5e7eb;
                    }}
                    .info-row:last-child {{
                        border-bottom: none;
                    }}
                    .info-label {{
                        font-weight: 600;
                        color: #4b5563;
                    }}
                    .info-value {{
                        color: #1f2937;
                        margin-top: 5px;
                    }}
                    .tips {{
                        background: #eff6ff;
                        border-left: 4px solid #3b82f6;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .tips-title {{
                        color: #1e40af;
                        font-weight: 600;
                        margin: 0 0 10px 0;
                    }}
                    .tips-text {{
                        color: #1e40af;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'></path>
                            </svg>
                        </div>
                        <h1>Your Child Started a Mission! 🚀</h1>
                    </div>
        
                    <p>Hello <strong>{sendTo}</strong>,</p>
        
                    <div class='status-container'>
                        <div class='status-icon'>⏱️</div>
                        <p style='margin: 0; font-size: 18px; color: #92400e; font-weight: 600;'>
                            {createdBy} has started working on a mission!
                        </p>
                    </div>
        
                    <div class='mission-info'>
                        <div class='info-row'>
                            <div class='info-label'>Mission:</div>
                            <div class='info-value'>{missionTitle}</div>
                        </div>
                        <div class='info-row'>
                            <div class='info-label'>Child:</div>
                            <div class='info-value'>{createdBy}</div>
                        </div>
                        <div class='info-row'>
                            <div class='info-label'>Status:</div>
                            <div class='info-value'>🔄 In Progress</div>
                        </div>
                    </div>
        
                    <div class='tips'>
                        <p class='tips-title'>💡 Parent Tips:</p>
                        <p class='tips-text'>
                            • Check in with your child to see how they're progressing<br>
                            • Offer support and encouragement<br>
                            • Remind them of the deadline<br>
                            • Celebrate their effort and learning
                        </p>
                    </div>
        
                    <p>You can track your child's progress anytime in your LearnLink dashboard. Keep supporting their learning journey!</p>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateMissionSubmittedEmailBody(string sendTo, string missionTitle, string createdBy)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Child Submitted Mission - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #a855f7, #9333ea);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .submission-container {{
                        background: linear-gradient(135deg, #e9d5ff, #ddd6fe);
                        padding: 25px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #a855f7;
                        text-align: center;
                    }}
                    .submission-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .submission-details {{
                        background: #f8fafc;
                        padding: 20px;
                        border-radius: 8px;
                        margin: 20px 0;
                        border: 1px solid #e2e8f0;
                    }}
                    .detail-row {{
                        padding: 10px 0;
                        border-bottom: 1px solid #e5e7eb;
                    }}
                    .detail-row:last-child {{
                        border-bottom: none;
                    }}
                    .detail-label {{
                        font-weight: 600;
                        color: #4b5563;
                    }}
                    .detail-value {{
                        color: #1f2937;
                        margin-top: 5px;
                    }}
                    .action-box {{
                        background: #f3e8ff;
                        border-left: 4px solid #a855f7;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .action-text {{
                        color: #6b21a8;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .cta {{
                        background: linear-gradient(135deg, #a855f7, #9333ea);
                        padding: 20px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: white;
                        color: #a855f7;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 10px 0;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 19l9 2-9-18-9 18 9-2zm0 0v-8'></path>
                            </svg>
                        </div>
                        <h1>Mission Submitted for Review! 📤</h1>
                    </div>
        
                    <p>Hello <strong>{sendTo}</strong>,</p>
        
                    <div class='submission-container'>
                        <div class='submission-icon'>✅</div>
                        <p style='margin: 0; font-size: 18px; color: #6b21a8; font-weight: 600;'>
                            {createdBy} has submitted their mission work!
                        </p>
                    </div>
        
                    <div class='submission-details'>
                        <div class='detail-row'>
                            <div class='detail-label'>Mission:</div>
                            <div class='detail-value'>{missionTitle}</div>
                        </div>
                        <div class='detail-row'>
                            <div class='detail-label'>Child:</div>
                            <div class='detail-value'>{createdBy}</div>
                        </div>
                        <div class='detail-row'>
                            <div class='detail-label'>Status:</div>
                            <div class='detail-value'>⏳ Awaiting Your Review</div>
                        </div>
                    </div>
        
                    <div class='action-box'>
                        <p class='action-text'>
                            <strong>Action Required:</strong> Please review {createdBy}'s submission and provide feedback. You can approve, reject, or request revisions through your LearnLink dashboard.
                        </p>
                    </div>
        
                    <div class='cta'>
                        <p style='color: white; margin: 0 0 15px 0;'>Review the submission now</p>
                        <a href='#' class='button'>Go to Dashboard</a>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        private string GenerateMissionReviewedEmailBody(string sendTo, string missionTitle, string reviewedBy, string status, string feedback, int? score)
        {
            var isApproved = status.Equals("approved", StringComparison.OrdinalIgnoreCase);
            var statusColor = isApproved ? "#10b981" : "#ef4444";
            var statusIcon = isApproved ? "✅" : "📝";
            var statusBg = isApproved ? "#d1fae5" : "#fee2e2";
            var statusBorder = isApproved ? "#10b981" : "#ef4444";

            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Your Mission Review - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, {statusColor}, {statusColor});
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .review-container {{
                        background: linear-gradient(135deg, {statusBg}, {statusBg});
                        padding: 25px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid {statusBorder};
                        text-align: center;
                    }}
                    .review-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .review-status {{
                        font-size: 20px;
                        font-weight: bold;
                        color: {statusColor};
                        margin: 0;
                    }}
                    .review-details {{
                        background: #f8fafc;
                        padding: 20px;
                        border-radius: 8px;
                        margin: 20px 0;
                        border: 1px solid #e2e8f0;
                    }}
                    .detail-row {{
                        padding: 10px 0;
                        border-bottom: 1px solid #e5e7eb;
                    }}
                    .detail-row:last-child {{
                        border-bottom: none;
                    }}
                    .detail-label {{
                        font-weight: 600;
                        color: #4b5563;
                    }}
                    .detail-value {{
                        color: #1f2937;
                        margin-top: 5px;
                    }}
                    .score-badge {{
                        display: inline-block;
                        background: linear-gradient(135deg, #fbbf24, #f59e0b);
                        color: white;
                        padding: 10px 20px;
                        border-radius: 20px;
                        font-weight: bold;
                        margin: 10px 0;
                    }}
                    .feedback-box {{
                        background: #fef3c7;
                        border-left: 4px solid #f59e0b;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .feedback-title {{
                        color: #92400e;
                        font-weight: 600;
                        margin: 0 0 10px 0;
                    }}
                    .feedback-text {{
                        color: #78350f;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z'></path>
                            </svg>
                        </div>
                        <h1>Your Mission Review is Ready! 🎓</h1>
                    </div>
        
                    <p>Hi <strong>{sendTo}</strong>! 👋</p>
        
                    <div class='review-container'>
                        <div class='review-icon'>{statusIcon}</div>
                        <p class='review-status'>
                            {status.ToUpper()}
                        </p>
                    </div>
        
                    <div class='review-details'>
                        <div class='detail-row'>
                            <div class='detail-label'>Mission:</div>
                            <div class='detail-value'>{missionTitle}</div>
                        </div>
                        <div class='detail-row'>
                            <div class='detail-label'>Reviewed by:</div>
                            <div class='detail-value'>{reviewedBy}</div>
                        </div>
                        <div class='detail-row'>
                            <div class='detail-label'>Your Score:</div>
                            <div class='detail-value'>
                                <span class='score-badge'>{score ?? 0}/100</span>
                            </div>
                        </div>
                    </div>
        
                    {(string.IsNullOrEmpty(feedback) ? "" : $@"
                    <div class='feedback-box'>
                        <p class='feedback-title'>💬 Feedback from {reviewedBy}:</p>
                        <p class='feedback-text'>{feedback}</p>
                    </div>
                    ")}
        
                    <p>
                        {(isApproved
                            ? "🎉 Congratulations! You did an amazing job! Your hard work paid off. Keep up this awesome effort!"
                            : "Thank you for your submission! Please review the feedback and feel free to try again. You've got this! 💪")}
                    </p>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string GenerateOtpEmailBody(string userName, string otp)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Your OTP Code - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #3b82f6, #2563eb);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .otp-container {{
                        background: linear-gradient(135deg, #dbeafe, #bfdbfe);
                        padding: 30px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #3b82f6;
                        text-align: center;
                    }}
                    .otp-label {{
                        color: #1e40af;
                        font-size: 14px;
                        font-weight: 600;
                        margin: 0 0 15px 0;
                    }}
                    .otp-code {{
                        background: white;
                        padding: 20px;
                        border-radius: 8px;
                        font-size: 32px;
                        font-weight: bold;
                        color: #3b82f6;
                        letter-spacing: 8px;
                        font-family: 'Courier New', monospace;
                        margin: 15px 0;
                    }}
                    .otp-expiry {{
                        color: #dc2626;
                        font-size: 12px;
                        font-weight: 600;
                        margin: 15px 0 0 0;
                    }}
                    .warning-box {{
                        background: #fef2f2;
                        border-left: 4px solid #dc2626;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .warning-title {{
                        color: #991b1b;
                        font-weight: 600;
                        margin: 0 0 10px 0;
                    }}
                    .warning-text {{
                        color: #7f1d1d;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .info-box {{
                        background: #eff6ff;
                        border-left: 4px solid #3b82f6;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .info-text {{
                        color: #1e40af;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z'></path>
                            </svg>
                        </div>
                        <h1>Your OTP Code</h1>
                    </div>
        
                    <p>Hi <strong>{userName}</strong>,</p>
        
                    <p>We received a request to verify your account. Use the code below to complete your verification:</p>
        
                    <div class='otp-container'>
                        <p class='otp-label'>Your One-Time Password (OTP)</p>
                        <div class='otp-code'>{otp}</div>
                        <p class='otp-expiry'>⏰ This code expires in 10 minutes</p>
                    </div>
        
                    <div class='warning-box'>
                        <p class='warning-title'>🔒 Security Notice:</p>
                        <p class='warning-text'>
                            Never share this code with anyone. LearnLink staff will never ask for your OTP code.
                        </p>
                    </div>
        
                    <div class='info-box'>
                        <p class='info-text'>
                            If you didn't request this code, please ignore this email or contact our support team immediately.
                        </p>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string GeneratePasswordResetConfirmationEmailBody(string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Password Reset Confirmation - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #10b981, #059669);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 24px;
                    }}
                    .success-container {{
                        background: linear-gradient(135deg, #d1fae5, #a7f3d0);
                        padding: 30px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #10b981;
                        text-align: center;
                    }}
                    .success-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .success-message {{
                        color: #065f46;
                        font-size: 18px;
                        font-weight: 600;
                        margin: 0;
                    }}
                    .info-box {{
                        background: #f0fdf4;
                        border-left: 4px solid #10b981;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .info-title {{
                        color: #065f46;
                        font-weight: 600;
                        margin: 0 0 10px 0;
                    }}
                    .info-text {{
                        color: #047857;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .security-tips {{
                        background: #eff6ff;
                        border-left: 4px solid #3b82f6;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .tips-title {{
                        color: #1e40af;
                        font-weight: 600;
                        margin: 0 0 10px 0;
                    }}
                    .tips-text {{
                        color: #1e40af;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z'></path>
                            </svg>
                        </div>
                        <h1>Password Reset Successful</h1>
                    </div>
        
                    <p>Hi <strong>{userName}</strong>,</p>
        
                    <div class='success-container'>
                        <div class='success-icon'>✅</div>
                        <p class='success-message'>Your password has been successfully reset!</p>
                    </div>
        
                    <div class='info-box'>
                        <p class='info-title'>What's Next?</p>
                        <p class='info-text'>
                            You can now log in to your LearnLink account with your new password. Your account is secure and ready to use.
                        </p>
                    </div>
        
                    <div class='security-tips'>
                        <p class='tips-title'>💡 Security Tips:</p>
                        <p class='tips-text'>
                            • Use a strong, unique password<br>
                            • Don't share your password with anyone<br>
                            • Log out when using shared devices<br>
                            • Enable two-factor authentication for extra security
                        </p>
                    </div>
        
                    <p>If you didn't request this password reset or have any concerns about your account security, please contact our support team immediately.</p>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

        public static string GenerateWelcomeEmailBody(string userName)
        {
            return $@"
            <!DOCTYPE html>
            <html lang='en'>
            <head>
                <meta charset='UTF-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Welcome to LearnLink - LearnLink</title>
                <style>
                    body {{
                        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                        line-height: 1.6;
                        color: #333;
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #f4f4f4;
                    }}
                    .container {{
                        background: white;
                        padding: 30px;
                        border-radius: 10px;
                        box-shadow: 0 0 20px rgba(0,0,0,0.1);
                    }}
                    .header {{
                        text-align: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        width: 60px;
                        height: 60px;
                        background: linear-gradient(135deg, #3b82f6, #2563eb);
                        border-radius: 50%;
                        display: inline-flex;
                        align-items: center;
                        justify-content: center;
                        margin-bottom: 20px;
                    }}
                    .logo svg {{
                        width: 30px;
                        height: 30px;
                        color: white;
                    }}
                    h1 {{
                        color: #2d3748;
                        margin: 0;
                        font-size: 28px;
                    }}
                    .welcome-container {{
                        background: linear-gradient(135deg, #dbeafe, #bfdbfe);
                        padding: 30px;
                        border-radius: 8px;
                        margin: 25px 0;
                        border-left: 4px solid #3b82f6;
                        text-align: center;
                    }}
                    .welcome-icon {{
                        font-size: 48px;
                        margin-bottom: 15px;
                    }}
                    .welcome-message {{
                        color: #1e40af;
                        font-size: 18px;
                        font-weight: 600;
                        margin: 0;
                    }}
                    .features {{
                        background: #f8fafc;
                        padding: 20px;
                        border-radius: 8px;
                        margin: 20px 0;
                        border: 1px solid #e2e8f0;
                    }}
                    .feature-title {{
                        color: #2d3748;
                        font-weight: 600;
                        margin: 0 0 15px 0;
                    }}
                    .feature-list {{
                        list-style: none;
                        padding: 0;
                        margin: 0;
                    }}
                    .feature-item {{
                        padding: 10px 0;
                        border-bottom: 1px solid #e5e7eb;
                        color: #4b5563;
                    }}
                    .feature-item:last-child {{
                        border-bottom: none;
                    }}
                    .feature-icon {{
                        margin-right: 10px;
                        color: #3b82f6;
                    }}
                    .cta {{
                        background: linear-gradient(135deg, #3b82f6, #2563eb);
                        padding: 20px;
                        border-radius: 8px;
                        text-align: center;
                        margin: 25px 0;
                    }}
                    .button {{
                        display: inline-block;
                        padding: 12px 24px;
                        background: white;
                        color: #3b82f6;
                        text-decoration: none;
                        border-radius: 6px;
                        font-weight: 600;
                        margin: 10px 0;
                    }}
                    .support-box {{
                        background: #fef3c7;
                        border-left: 4px solid #f59e0b;
                        padding: 15px;
                        margin: 20px 0;
                        border-radius: 4px;
                    }}
                    .support-text {{
                        color: #78350f;
                        margin: 0;
                        font-size: 14px;
                    }}
                    .footer {{
                        margin-top: 30px;
                        padding-top: 20px;
                        border-top: 1px solid #e5e7eb;
                        text-align: center;
                        color: #6b7280;
                        font-size: 14px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>
                            <svg fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                                <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M13 10V3L4 14h7v7l9-11h-7z'></path>
                            </svg>
                        </div>
                        <h1>Welcome to LearnLink! 🎉</h1>
                    </div>
        
                    <p>Hi <strong>{userName}</strong>,</p>
        
                    <div class='welcome-container'>
                        <div class='welcome-icon'>👋</div>
                        <p class='welcome-message'>We're thrilled to have you join our learning community!</p>
                    </div>
        
                    <p>LearnLink is an innovative platform designed to make learning engaging, rewarding, and fun. Whether you're a parent guiding your child's education or a child embarking on exciting learning missions, we're here to support your journey.</p>
        
                    <div class='features'>
                        <p class='feature-title'>✨ What You Can Do:</p>
                        <ul class='feature-list'>
                            <li class='feature-item'><span class='feature-icon'>🎯</span> Create and manage educational missions</li>
                            <li class='feature-item'><span class='feature-icon'>⭐</span> Earn and track points for achievements</li>
                            <li class='feature-item'><span class='feature-icon'>🏆</span> Redeem points for exciting rewards</li>
                            <li class='feature-item'><span class='feature-icon'>📊</span> Monitor progress and performance</li>
                            <li class='feature-item'><span class='feature-icon'>🛍️</span> Access our exclusive reward shop</li>
                        </ul>
                    </div>
        
                    <div class='cta'>
                        <p style='color: white; margin: 0 0 15px 0;'>Ready to get started?</p>
                        <a href='#' class='button'>Go to Dashboard</a>
                    </div>
        
                    <div class='support-box'>
                        <p class='support-text'>
                            <strong>Need Help?</strong> Our support team is here to assist you. Visit our help center or contact us at support@learnlink.com
                        </p>
                    </div>
        
                    <div class='footer'>
                        <p>Best regards,<br>
                        <strong>LearnLink Team</strong></p>
                        <p>This is an automated message, please do not reply to this email.</p>
                    </div>
                </div>
            </body>
            </html>";
        }

    }
}
