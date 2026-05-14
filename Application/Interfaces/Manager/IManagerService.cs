using Application.DTOs.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Manager
{
    public interface IManagerService
    {
        Task<DashboardOverviewDto> GetOverviewDtoAsync();
        Task<IEnumerable<UserOverviewDto>> GetUserOverviewDtoAsync();
        Task<IEnumerable<ParentPerformanceDto>> GetParentPerformanceDtoAsync();
        Task<IEnumerable<ChildPerformanceDto>> GetChildPerformanceDtoAsync();
        Task<IEnumerable<CompletionChartDto>> GetCompletionTrend();
        Task<IEnumerable<ActivityChartDto>> GetWeeklyActivity();
    }
}
