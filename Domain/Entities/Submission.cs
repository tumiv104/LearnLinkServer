using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Submission
	{
		[Key]
		public int SubmissionId { get; set; }

		[ForeignKey("Mission")]
		public int MissionId { get; set; }
		public Mission Mission { get; set; }

		[ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }

		public string FileUrl { get; set; }
		public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
		public SubmissionStatus Status { get; set; } // pending | approved | rejected
		public string Feedback { get; set; }
		public int? Score { get; set; }
		public DateTime? ReviewedAt { get; set; }
	}

	public enum SubmissionStatus
	{
		Pending = 0,
		Approved = 1,
		Rejected = 2
	}
}
