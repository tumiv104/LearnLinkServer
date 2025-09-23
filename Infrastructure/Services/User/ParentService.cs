using Application.DTOs.User;
using Application.Interfaces;
using Application.Interfaces.User;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class ParentService : IParentService
{
    private readonly LearnLinkDbContext _context;

    public ParentService(LearnLinkDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChildBasicInfoDTO>> GetChildrenAsync(string parentEmail)
    {
        var parent = await _context.Users
            .Include(u => u.ParentRelations)
            .ThenInclude(pc => pc.Child)
            .FirstOrDefaultAsync(u => u.Email == parentEmail && u.RoleId == (int)RoleEnum.Parent);

        if (parent == null) return new List<ChildBasicInfoDTO>();

        var children = parent.ParentRelations.Select(pc => pc.Child).ToList();

        return children.Select(c => new ChildBasicInfoDTO
        {
            ChildId = c.userId,
            Name = c.Name,
            Email = c.Email,
            Dob = c.Dob,
            AvatarUrl = c.AvatarUrl,
            CreatedAt = c.CreatedAt
        }).ToList();
    }
}
