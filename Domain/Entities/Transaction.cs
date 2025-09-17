using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Transaction
	{
		[Key]
		public int TransactionId { get; set; }

		[ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }

		[ForeignKey("Mission")]
		public int? MissionId { get; set; }
		public Mission Mission { get; set; }

		[ForeignKey("Reward")]
		public int? RewardId { get; set; }
		public Reward Reward { get; set; }

		public int Amount { get; set; }
		public TransactionType Type { get; set; } // mission_reward | redeem | adjust | purchase
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public enum TransactionType
	{
        MissionReward,
		Redeem,
		Adjust,
		Purchase
	}
}
