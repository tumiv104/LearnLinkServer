using Application.DTOs.Shop;
using Application.Interfaces.Shop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Shop
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShopController : BaseController
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllShops()
        {
            var result = await _shopService.GetAllShopAsync();
            return OkResponse(result);
        }

        [HttpGet("getPaging")]
        public async Task<IActionResult> GetShops(int page = 1, int pageSize = 10)
        {
            var result = await _shopService.GetShopsAsync(page, pageSize);
            return OkResponse(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateShop([FromBody] ShopRequestDTO dto)
        {
            var shop = await _shopService.CreateShopAsync(dto);
            return OkResponse(shop);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopRequestDTO dto)
        {
            var shop = await _shopService.UpdateShopAsync(id, dto);
            if (shop == null) return NotFoundResponse();
            return OkResponse(shop);
        }

        [HttpDelete("disable/{id}")]
        public async Task<IActionResult> DisableShop(int id)
        {
            var success = await _shopService.DisableShopAsync(id);
            if (!success) return NotFoundResponse();
            return NoContent();
        }
    }
}
