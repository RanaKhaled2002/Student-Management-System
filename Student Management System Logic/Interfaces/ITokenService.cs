using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Logic.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(IdentityUser user, UserManager<IdentityUser> userManager);
        Task RevokeTokenAsync(string token, DateTime expiration);
        Task<bool> IsTokenRevokedAsync(string token);
    }
}
