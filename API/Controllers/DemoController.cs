using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemoController : BaseController
    {
        [HttpGet("publicdata")]
        public IActionResult Get()
        {
            return OkResponse<object>(null, "anyone can access this");
        }

        [HttpGet("adminonly")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return OkResponse<object>(null, "only admin can access this");
        }

        [HttpGet("parentonly")]
        [Authorize(Roles = "Parent")]
        public IActionResult ParentOnly()
        {
            return OkResponse<object>(null, "only parent can access this");
        }

        [HttpGet("childParentonly")]
        [Authorize(Roles = "Child, Parent")]
        public IActionResult ChildOnly()
        {
            return OkResponse<object>(null, "only child or parent can access this");
        }
    }
}
