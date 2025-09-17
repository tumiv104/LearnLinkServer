using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
	public class ParentChild
	{
		[Key]
		public int RealtionId { get; set; }
		[ForeignKey("Parent")]
		public int ParentId { get; set; }
		public User Parent { get; set; }
		[ForeignKey("Child")]
		public int ChildId { get; set; }
		public User Child { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	}
}
