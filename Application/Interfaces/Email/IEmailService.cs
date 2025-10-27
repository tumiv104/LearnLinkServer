using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Email
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otp, string userName);
        Task<bool> SendPasswordResetConfirmationAsync(string email, string userName);
        Task<bool> SendWelcomeEmailAsync(string email, string userName);
        Task<bool> SendMissionCreatedEmailAsync(string email, string createBy, string sendTo, string missionTitle, string deadline);
        Task<bool> SendMissionStartedEmailAsync(string email, string createBy, string sendTo, string missionTitle);
        Task<bool> SendMissionSubmittedEmailAsync(string email, string createBy, string sendTo, string missionTitle);
        Task<bool> SendMissionReviewedEmailAsync(string email, string createBy, string sendTo, string missionTitle, string status, string feedback, int? score);
        Task<bool> SendPasswordResetEmailAsync(string email, string resetLink, string userName);


    }
}
