using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Payment
	{
		[Key]
		public int PaymentId { get; set; }

		[ForeignKey("Parent")]
		public int ParentId { get; set; }
		public User Parent { get; set; }
		[Column(TypeName = "decimal(18,2)")]
		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string Method { get; set; } // MoMo | ZaloPay | Visa | Mastercard
		public PaymentStatus Status { get; set; } // pending | success | failed
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

	public enum PaymentStatus
	{
		Pending = 0,
		Success = 1,
		Failed = 2
	}
}
