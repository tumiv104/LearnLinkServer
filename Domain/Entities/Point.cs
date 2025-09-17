using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class Point
	{
		[Key]
		public int PointsId { get; set; }

		[ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }

		public int Balance { get; set; }
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	}
}
