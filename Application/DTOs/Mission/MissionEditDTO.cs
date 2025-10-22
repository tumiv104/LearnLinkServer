using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
	public class MissionEditDTO
	{
		public string? Title { get; set; }
		public string? Description { get; set; }
		public int? Points { get; set; }
		public string? Promise { get; set; }
		public string? Punishment { get; set; }
		public DateTime? Deadline { get; set; }
		public string? AttachmentUrl { get; set; }
	}

}
