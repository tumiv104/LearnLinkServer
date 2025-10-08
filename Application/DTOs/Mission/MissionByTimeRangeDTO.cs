using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
	public class MissionByTimeRangeDTO
	{
		public List<MissionResponseDTO> TodayMissions { get; set; } = new();
		public List<MissionResponseDTO> WeekMissions { get; set; } = new();
		public List<MissionResponseDTO> MonthMissions { get; set; } = new();
	}

}
