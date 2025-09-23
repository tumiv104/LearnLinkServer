using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Domain.Entities
{
    public class Mission
    {
        [Key]
        public int MissionId { get; set; }

        [ForeignKey("Parent")]
        public int ParentId { get; set; }
        public User Parent { get; set; }

        [ForeignKey("Child")]
        public int ChildId { get; set; }
        public User Child { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        public int Points { get; set; }

        public string? Promise { get; set; } // nullable
        //public int? BonusPoints { get; set; } // nullable
        public string? Punishment { get; set; } // nullable

        public DateTime? Deadline { get; set; }
        public string AttachmentUrl { get; set; }
        public MissionStatus Status { get; set; } // assigned | submitted | reviewed | completed

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }

    public enum MissionStatus
    {
        Assigned = 0,
        Submitted = 1,
        Processing = 2,
        Completed = 3
    }
}
