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
    [HttpGet("{parentId}/children")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetChildren(int parentId)
    {
        var children = await _parentService.GetChildrenAsync(parentId);
        return OkResponse(children, "List of your children");
    }

    [HttpPost("{parentId}/children")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> CreateChild(
        int parentId,
       [FromForm] ChildCreateDTO childDTO,
       [FromServices] IFileStorage fileStorage,
       [FromServices] IWebHostEnvironment env)
    {
        if (childDTO.AvatarFile != null)
        {
            using var stream = childDTO.AvatarFile.OpenReadStream();
            var url = await fileStorage.SaveAsync(stream, childDTO.AvatarFile.FileName, "avatars", env.WebRootPath);
            childDTO.AvatarUrl = url;
        }

        var result = await _parentService.CreateChildAsync(parentId, childDTO);
        if (result != "")
            return BadRequestResponse(result); 

        return OkResponse(result);
    }


    [HttpGet("{parentId}/children/{childId}")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> GetChildProfile(int parentId, int childId)
    {
        var profile = await _parentService.GetChildProfileAsync(parentId, childId);
        if (profile == null)
            return NotFoundResponse("Child not found or does not belong to this parent");

        return OkResponse(profile, "Child profile retrieved successfully");
    }




}
