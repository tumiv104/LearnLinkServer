using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Notification
	{
		[Key]
		public int NotificationId { get; set; }

		[ForeignKey("User")]
		public int UserId { get; set; }
		public User User { get; set; }

		public string Message { get; set; }
		public NotificationType Type { get; set; } // mission_assigned | mission_reviewed | reward_redeem
		public bool IsRead { get; set; } = false;
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public enum NotificationType
	{
        MissionAssigned,
        MissionReviewed,
		RewardRedeem
	}
}
