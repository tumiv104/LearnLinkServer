using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Submission
{
    public class RejectSubmissionDTO
    {
        public int SubmissionId { get; set; }
        public string Feedback { get; set; }
    }
}
