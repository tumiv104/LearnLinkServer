using Application.DTOs.Common;
using Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Product
{
    public interface IProductService
    {
        Task<List<ProductResponseDTO>> GetAllProductAsync();
        Task<PageResultDTO<ProductResponseDTO>> GetProductsAsync(int pageNumber, int pageSize); 
        Task<ProductResponseDTO> CreateProductAsync(ProductRequestDTO dto); 
        Task<ProductResponseDTO?> UpdateProductAsync(int productId, ProductRequestDTO dto); 
        Task<bool> DeleteProductAsync(int productId);
    }
}
