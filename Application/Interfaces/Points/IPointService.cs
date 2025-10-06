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
        Task<bool> ChildRedeemProduct(RedeemProductDTO dto);
        Task<List<RedemptionResponseDTO>> GetAllRedemptionsByChildAsync(int childId);
        Task<List<RedemptionResponseDTO>> GetAllRedemptionsAsync();
        Task<RedemptionResponseDTO> UpdateRedemptionStatus(int redemptionId, string newStatus);
    }
}
