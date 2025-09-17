using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Role
	{
		[Key]
		public int RoleId { get; set; }
		[Required, MaxLength(50)]
		public string Name { get; set; }
		// Navigation property
		public ICollection<User> Users { get; set; }
	}

	public enum RoleEnum
	{
		Admin = 1,
		Parent = 2,
		Child = 3
	}
}
