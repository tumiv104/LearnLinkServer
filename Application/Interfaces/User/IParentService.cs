using Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.User
{
    public interface IParentService
    {
        Task<List<ChildBasicInfoDTO>> GetChildrenAsync(int parentId);
        Task<bool> CreateChildAsync(int parentId, ChildCreateDTO childDTO);
        Task<UserProfileDTO?> GetChildProfileAsync(int parentId, int childId);
    }
}
