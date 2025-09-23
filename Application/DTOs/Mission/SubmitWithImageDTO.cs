using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
	public class SubmitWithImageDTO
	{
		[Required]
		public int MissionId { get; set; }

		[Required]
		public string Feedback { get; set; }

			
	}

}
