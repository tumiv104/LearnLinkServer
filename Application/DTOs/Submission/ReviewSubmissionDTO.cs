using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Submission
{
    public class ReviewSubmissionDTO
    {
        public int SubmissionId { get; set; }
        public int Score { get; set; }
        public string Feedback { get; set; }
    }
}
