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
        public NotificationType Type { get; set; }
        public string Payload { get; set; } = "{}";
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

	public enum NotificationType
	{
        MissionAssigned = 0,
        MissionStarted = 1,
        MissionSubmitted = 2,
        MissionReviewed = 3,
        RewardRedeem = 4
    }
}
