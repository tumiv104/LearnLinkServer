using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Redemption
	{
		[Key]
		public int RedemptionId { get; set; }

		[ForeignKey("Reward")]
		public int? RewardId { get; set; }
		public Reward Reward { get; set; } = null!;

        [ForeignKey("Product")] 
		public int? ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }

        public int PointsSpent { get; set; }

        public RedemptionStatus Status { get; set; } // pending | confirmed | cancelled | delivered
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public enum RedemptionStatus
	{
		Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
		Delivered = 3
	}
}
