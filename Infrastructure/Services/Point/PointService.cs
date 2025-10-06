using Application.DTOs.Points;
using Application.DTOs.Shop;
using Application.Interfaces.Points;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Points
{
    public class PointService : IPointService
    {
        private readonly LearnLinkDbContext _context;

        public PointService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ChildRedeemProduct(RedeemProductDTO dto)
        {
            var point = await _context.Points.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == dto.ChildId);
            if (point == null) return false;
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == dto.ProductId);
            if (product == null || product.Stock == 0) return false;
            if (point.Balance < product.PricePoints) return false;
            var redemption = new Redemption
            {
                ProductId = product.ProductId,
                ChildId = dto.ChildId,
                PointsSpent = product.PricePoints,
                Status = RedemptionStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Redemptions.Add(redemption);
            product.Stock -= 1;
            point.Balance -= product.PricePoints;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<RedemptionResponseDTO>> GetAllRedemptionsAsync()
        {
            var query = _context.Redemptions.Include(r => r.Child).Include(r => r.Product).ThenInclude(p => p.Shop).AsQueryable();
            var redemptions = await query
                .OrderBy(s => s.RedemptionId)
                .Select(s => new RedemptionResponseDTO
                {
                    RedemptionId = s.RedemptionId,
                    ProductId = s.Product.ProductId,
                    ProductName = s.Product.Name,
                    ProductImage = s.Product.ImageUrl,
                    ShopName = s.Product.Shop.ShopName,
                    PointsSpent = s.PointsSpent,
                    ChildId = s.ChildId,
                    ChildName = s.Child.Name,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt,
                })
                .ToListAsync();
            return redemptions;
        }

        public async Task<List<RedemptionResponseDTO>> GetAllRedemptionsByChildAsync(int id)
        {
            var query = _context.Redemptions.Include(r => r.Child).Include(r => r.Product).ThenInclude(p => p.Shop).Where(r => r.ChildId == id).AsQueryable();
            var redemptions = await query
                .OrderBy(s => s.RedemptionId)
                .Select(s => new RedemptionResponseDTO
                {
                    RedemptionId = s.RedemptionId,
                    ProductId = s.Product.ProductId,
                    ProductName = s.Product.Name,
                    ProductImage = s.Product.ImageUrl,
                    ShopName = s.Product.Shop.ShopName,
                    PointsSpent = s.PointsSpent,
                    ChildId = s.ChildId,
                    ChildName = s.Child.Name,
                    Status = s.Status.ToString(),
                    CreatedAt = s.CreatedAt,
                })
                .ToListAsync();
            return redemptions;
        }

        public async Task<PointDTO> GetPointByUserId(int userId)
        {
            var point = await _context.Points.Include(p => p.User).ThenInclude(u => u.Role).FirstOrDefaultAsync(p  => p.UserId == userId);
            if (point == null) return null;
            return new PointDTO
            {
                UserId = userId,
                UserName = point.User.Name,
                UserRole = point.User.Role.Name,
                UserEmail = point.User.Email,
                Balance = point.Balance
            };
        }

        public async Task<RedemptionResponseDTO> UpdateRedemptionStatus(int redemptionId, string newStatus)
        {
            if (newStatus == "Pending") return null;
            var redemption = await _context.Redemptions.FirstOrDefaultAsync(r => r.RedemptionId == redemptionId);
            if (redemption == null) return null;
            if (newStatus == "Confirmed")
            {
                if (redemption.Status == RedemptionStatus.Cancelled || redemption.Status == RedemptionStatus.Delivered)
                    return null;
                redemption.Status = RedemptionStatus.Confirmed;
            }
            if (newStatus == "Delivered")
            {
                if (redemption.Status == RedemptionStatus.Cancelled) return null;
                redemption.Status = RedemptionStatus.Delivered;
            }
            if (newStatus == "Cancelled")
            {
                var point = await _context.Points.FirstOrDefaultAsync(p => p.UserId == redemption.ChildId);
                point.Balance += redemption.PointsSpent;
                redemption.Status = RedemptionStatus.Cancelled;
            }
            await _context.SaveChangesAsync();
            return new RedemptionResponseDTO
            {
                RedemptionId = redemption.RedemptionId,
                PointsSpent = redemption.PointsSpent,
                ChildId = redemption.ChildId,
                Status = redemption.Status.ToString(),
                CreatedAt = redemption.CreatedAt,
            };
        }
    }
}
