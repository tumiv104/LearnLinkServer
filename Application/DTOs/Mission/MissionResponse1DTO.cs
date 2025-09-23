using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
	public class MissionResponse1DTO
	{
		public int MissionId { get; set; }
		public int ParentId { get; set; }
		public string ParentName { get; set; }
		public int ChildId { get; set; }
		public string ChildName { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public int Points { get; set; }
		public string? Promise { get; set; }
		//public int? BonusPoints { get; set; }
		public string? Punishment { get; set; }
		public DateTime? Deadline { get; set; }
		public string AttachmentUrl { get; set; }
		public MissionStatus Status { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public string ImageUrl { get; set; }
		public int SerialNumber { get; set; }
		public string Feedback { get; set; }
	}
}
