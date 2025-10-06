using Application.DTOs.Common;
using Application.DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Shop
{
    public interface IShopService
    {
        Task<List<ShopResponeDTO>> GetAllShopAsync();
        Task<PageResultDTO<ShopResponeDTO>> GetShopsAsync(int pageNumber, int pageSize); 
        Task<ShopResponeDTO> CreateShopAsync(ShopRequestDTO dto); 
        Task<ShopResponeDTO?> UpdateShopAsync(int shopId, ShopRequestDTO dto); 
        Task<bool> DisableShopAsync(int shopId);
    }
}
