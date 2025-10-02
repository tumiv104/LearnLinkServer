using Application.DTOs.Dashboard;
using Application.DTOs.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        Task<ParentOverviewDTO> GetParentOverviewAsync(int parentId);
    }
}
