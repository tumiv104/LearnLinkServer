using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Redemption
	{
		[Key]
		public int RedemptionId { get; set; }

		[ForeignKey("Reward")]
		public int RewardId { get; set; }
		public Reward Reward { get; set; }

		[ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }

		public RedemptionStatus Status { get; set; } // pending | approved | rejected | delivered
		public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
		public DateTime? ApprovedAt { get; set; }
	}

	public enum RedemptionStatus
	{
		Pending,
		Approved,
		Rejected,
		Delivered
	}
}
