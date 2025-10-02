using Application.DTOs.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Points
{
    public interface IPointService
    {
        Task<PointDTO> GetPointByUserId(int userId);
    }
}
