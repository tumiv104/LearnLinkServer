using API.Controllers;
using Application.Interfaces;
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

}
