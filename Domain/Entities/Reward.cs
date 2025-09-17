using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Reward
	{
		[Key]
		public int RewardId { get; set; }

		[ForeignKey("Parent")]
		public int ParentId { get; set; }
		public User Parent { get; set; }

		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public int Cost { get; set; }
		public string Type { get; set; } // virtual | physical
		public int? Stock { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ICollection<Redemption> Redemptions { get; set; }
		public ICollection<Transaction> Transactions { get; set; }
	}
}
