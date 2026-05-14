using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Mission
{
    public class AssignMissionResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }

        public AssignMissionResult(bool success, string? message)
        {
            Success = success;
            Message = message;
        }
    }

}
