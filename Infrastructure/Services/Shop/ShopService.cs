using Application.DTOs.Common;
using Application.DTOs.Shop;
using Application.Interfaces.Shop;
using Azure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Shop
{
    public class ShopService : IShopService
    {
        private readonly LearnLinkDbContext _context;

        public ShopService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<ShopResponeDTO> CreateShopAsync(ShopRequestDTO dto)
        {
            var shop = new Domain.Entities.Shop
            {
                ShopName = dto.ShopName,
                ContactInfo = dto.ContactInfo,
                Website = dto.Website,
                IsActive = true
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            return new ShopResponeDTO
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ContactInfo = shop.ContactInfo,
                Website = shop.Website,
                IsActive = shop.IsActive
            };
        }

        public async Task<bool> DisableShopAsync(int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return false;

            shop.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ShopResponeDTO>> GetAllShopAsync()
        {
            var query = _context.Shops.AsQueryable();
            var shops = await query
                .OrderBy(s => s.ShopId)
                .Select(s => new ShopResponeDTO
                {
                    ShopId = s.ShopId,
                    ShopName = s.ShopName,
                    ContactInfo = s.ContactInfo,
                    Website = s.Website,
                    IsActive = s.IsActive
                })
                .ToListAsync();
            return shops;
        }

        public async Task<PageResultDTO<ShopResponeDTO>> GetShopsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Shops.AsQueryable();
            var totalCount = await query.CountAsync();

            var shops = await query
                .OrderBy(s => s.ShopId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShopResponeDTO
                {
                    ShopId = s.ShopId,
                    ShopName = s.ShopName,
                    ContactInfo = s.ContactInfo,
                    Website = s.Website,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return new PageResultDTO<ShopResponeDTO>
            {
                Items = shops,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ShopResponeDTO?> UpdateShopAsync(int shopId, ShopRequestDTO dto)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null) return null;

            shop.ShopName = dto.ShopName;
            shop.ContactInfo = dto.ContactInfo;
            shop.Website = dto.Website;
            shop.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new ShopResponeDTO
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ContactInfo = shop.ContactInfo,
                Website = shop.Website,
                IsActive = shop.IsActive
            };
        }
    }
}
