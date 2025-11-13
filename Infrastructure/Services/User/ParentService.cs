using Application.DTOs.User;
using Application.Interfaces;
using Application.Interfaces.User;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Services.User;
using Microsoft.EntityFrameworkCore;

public class ParentService : IParentService
{
    private readonly LearnLinkDbContext _context;
    private readonly IUserService _userService;

    public ParentService(IUserService userService, LearnLinkDbContext context)
    {
        _userService = userService;
        _context = context;
    }

    public async Task<List<ChildBasicInfoDTO>> GetChildrenAsync(int parentId)
    {
        var parent = await _context.Users
            .Include(u => u.ParentRelations)
            .ThenInclude(pc => pc.Child)
            .FirstOrDefaultAsync(u => u.userId == parentId && u.RoleId == (int)RoleEnum.Parent);

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

    public async Task<string> CreateChildAsync(int parentId, ChildCreateDTO childDTO)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(
            childDTO.Email ?? "",
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$"
        ))
            return "Invalid email format.";

        if (string.IsNullOrWhiteSpace(childDTO.Password) || childDTO.Password.Length < 6 || childDTO.Password.Contains(" "))
            return "Password must be at least 6 characters and contain no spaces.";

        if (await _context.Users.AnyAsync(u => u.Email == childDTO.Email))
            return "Email already exists.";

        var parent = await _context.Users
            .Include(p => p.ParentRelations)
            .FirstOrDefaultAsync(p => p.userId == parentId);

        if (parent == null)
            return "Parent not found.";

        // ❗ Giới hạn con nếu chưa Premium
        if (!parent.IsPremium)
        {
            var currentChildrenCount = parent.ParentRelations?.Count ?? 0;
            if (currentChildrenCount >= 1)
                return "You have reached the child limit. Upgrade to Premium to add more children.";
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var child = new User
            {
                Name = childDTO.Name,
                Email = childDTO.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(childDTO.Password),
                Dob = childDTO.Dob,
                AvatarUrl = childDTO.AvatarUrl,
                RoleId = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var relation = new ParentChild
            {
                ParentId = parentId,
                Child = child,
                CreatedAt = DateTime.UtcNow
            };

            var point = new Point
            {
                User = child,
                Balance = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(child);
            await _context.ParentChildren.AddAsync(relation);
            await _context.Points.AddAsync(point);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return "";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return "An error occurred while creating the child: " + ex.Message;
        }
    }

    public async Task<UserProfileDTO?> GetChildProfileAsync(int parentId, int childId)
    {
        var isChildOfParent = await _context.ParentChildren
            .AnyAsync(pc => pc.ParentId == parentId && pc.ChildId == childId);

        if (!isChildOfParent)
            return null;

        return await _userService.GetUserProfileAsync(childId);
    }

}
