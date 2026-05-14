using Application.DTOs.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Report
{
    public interface IReportService
    {
        Task<ChildProgressReportDTO> GetChildProgressAsync(int parentId, int childId, string period = "all");
    }
}
