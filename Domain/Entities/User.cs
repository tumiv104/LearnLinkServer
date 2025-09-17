using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class User
	{
		[Key]
		public int userId { get; set; }
		[ForeignKey("Role")]
		public int RoleId { get; set; }
		public Role Role{ get; set; }
		[Required, MaxLength(100)]
		public string Name { get; set; }
		[Required, MaxLength(150)]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
		public DateTime? Dob { get; set; }
		public string? AvatarUrl { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		// Navigation properties
		public ICollection<ParentChild> ParentRelations { get; set; }
		public ICollection<ParentChild> ChildRelations { get; set; }
		public ICollection<Mission> MissionAsParent { get; set; }
		public ICollection<Mission> MissionAsChild { get; set; }
		public ICollection<Notification> Notifications { get; set; }
		public ICollection<Submission> Submissions { get; set; }
		public ICollection<Point> Points { get; set; }
		public ICollection<Transaction> Transactions { get; set; }
		public ICollection<Reward> Rewards { get; set; }
		public ICollection<Redemption> Redemptions { get; set; }
		public ICollection<Payment> Payments { get; set; }
	}
}
