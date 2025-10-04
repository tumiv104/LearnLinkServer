using API.Controllers;
using Application.DTOs.User;
using Application.Interfaces;
using Application.Interfaces.Common;
using Application.Interfaces.User;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class ParentController : BaseController
{
    private readonly IParentService _parentService;

    public ParentController(IParentService parentService)
    {
        _parentService = parentService;
    }

    // Parent xem danh sách các con của mình
    [HttpGet("children")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetChildren()
    {
        var parentIdClaim = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(parentIdClaim))
            return UnauthorizedResponse();

        var parentId = int.Parse(parentIdClaim);

        var children = await _parentService.GetChildrenAsync(parentId);
        return OkResponse(children, "List of your children");
    }

    [HttpPost("children")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CreateChild([FromForm] ChildCreateDTO childDTO, [FromServices] IFileStorage fileStorage, [FromServices] IWebHostEnvironment env)
    {
        var parentIdClaim = User.FindFirstValue("id");
        if (string.IsNullOrEmpty(parentIdClaim))
            return UnauthorizedResponse();

        var parentId = int.Parse(parentIdClaim);

        // Upload avatar nếu có
        if (childDTO.AvatarFile != null)
        {
            using var stream = childDTO.AvatarFile.OpenReadStream();
            var url = await fileStorage.SaveAsync(stream, childDTO.AvatarFile.FileName, "avatars", env.WebRootPath);
            childDTO.AvatarUrl = url;
        }
        var success = await _parentService.CreateChildAsync(parentId, childDTO);
        if (!success) return BadRequestResponse("Failed to create child");

        return OkResponse<object>(null, "Child created successfully");
    }


}
