using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Submission
{
    public class SubmissionResponseDTO
    {
        public int SubmissionId { get; set; }
        public int MissionId { get; set; }
        public int ChildId { get; set; }
        public string FileUrl { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string Status { get; set; }
        public string Feedback { get; set; }
        public int? Score { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
