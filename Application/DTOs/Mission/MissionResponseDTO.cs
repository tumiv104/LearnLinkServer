using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
    public class MissionResponseDTO
    {
        public int MissionId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public int Points { get; set; }
        public DateTime? Deadline { get; set; }
        public string Status { get; set; }           
        //public string StatusName { get; set; }    
        public DateTime CreatedAt { get; set; }
        public int ChildId { get; set; }      
        public string ChildName { get; set; }
        public int SerialNumber { get; set; }
    }

}

