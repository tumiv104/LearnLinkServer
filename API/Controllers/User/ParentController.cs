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
        var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
        if (string.IsNullOrEmpty(email)) return UnauthorizedResponse();

        var children = await _parentService.GetChildrenAsync(email);
        return OkResponse(children, "List of your children");
    }
}
