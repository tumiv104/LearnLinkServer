using Application.DTOs.Product;
using Application.Interfaces.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Product
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : BaseController
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("getAll")]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _productService.GetAllProductAsync();
            return OkResponse(result);
        }

        [HttpGet("getPaging")]
        public async Task<IActionResult> GetProducts(int page = 1, int pageSize = 10)
        {
            var result = await _productService.GetProductsAsync(page, pageSize);
            return OkResponse(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequestDTO dto)
        {
            var product = await _productService.CreateProductAsync(dto);
            return OkResponse(product);
        }

        [HttpPut("edit/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductRequestDTO dto)
        {
            var product = await _productService.UpdateProductAsync(id, dto);
            return OkResponse(product);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var success = await _productService.DeleteProductAsync(id);
            if (!success) return NotFoundResponse();
            return NoContent();
        }
    }
}
