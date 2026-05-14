using Application.DTOs.Common;
using Application.DTOs.Product;
using Application.Interfaces.Product;
using Azure;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly LearnLinkDbContext _context;

        public ProductService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponseDTO> CreateProductAsync(ProductRequestDTO dto)
        {
            var product = new Domain.Entities.Product
            {
                ShopId = dto.ShopId,
                Name = dto.Name,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                PricePoints = dto.PricePoints,
                Stock = dto.Stock,
                IsActive = dto.IsActive
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return new ProductResponseDTO
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                Name = product.Name,
                Description = product.Description,
                ImageUrl= product.ImageUrl,
                PricePoints = product.PricePoints,
                Stock = product.Stock,
                IsActive = product.IsActive
            };
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<ProductResponseDTO>> GetAllProductAsync()
        {
            var query = _context.Products.Include(p => p.Shop).AsQueryable();
            var products = await query
                .Select(p => new ProductResponseDTO
                {
                    ProductId = p.ProductId,
                    ShopId = p.ShopId,
                    ShopName = p.Shop.ShopName,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PricePoints = p.PricePoints,
                    Stock = p.Stock,
                    IsActive = p.IsActive
                })
                .ToListAsync();
            return products;
        }

        public async Task<PageResultDTO<ProductResponseDTO>> GetProductsAsync(int page, int pageSize)
        {
            var query = _context.Products.AsQueryable();
            var totalCount = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.ProductId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductResponseDTO
                {
                    ProductId = p.ProductId,
                    ShopId = p.ShopId,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PricePoints = p.PricePoints,
                    Stock = p.Stock,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            return new PageResultDTO<ProductResponseDTO>
            {
                Items = products,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<ProductResponseDTO?> UpdateProductAsync(int productId, ProductRequestDTO dto)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return null;

            product.ShopId = dto.ShopId;
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.ImageUrl = dto.ImageUrl;
            product.PricePoints = dto.PricePoints;
            product.Stock = dto.Stock;
            product.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return new ProductResponseDTO
            {
                ProductId = product.ProductId,
                ShopId = product.ShopId,
                Name = product.Name,
                Description = product.Description,
                ImageUrl = product.ImageUrl,
                PricePoints = dto.PricePoints,
                Stock = dto.Stock,
                IsActive = dto.IsActive,
            };
        }
    }
}
