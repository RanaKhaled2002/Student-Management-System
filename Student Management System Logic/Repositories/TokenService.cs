using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.Models;
using Student_Management_System_Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Logic.Repositories
{
    public class TokenService : ITokenService
    {
        // استخدمت ده عشان اعرف اقرأ الاعدادات زي issuer,key
        private readonly IConfiguration _config;
        private readonly StudentDbContext _dbContext;

        public TokenService(IConfiguration config,StudentDbContext dbContext)
        {
            _config = config;
            _dbContext = dbContext;
        }
                                                   // اللي عمل login    //اجيب ال role بتاعته
        public async Task<string> CreateTokenAsync(IdentityUser user, UserManager<IdentityUser> userManager)
        {
            // معلومات هخزنها جوا ال token
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // عشان معملش token متكرر
                new Claim(ClaimTypes.Email, user.Email)
            };

            // لازم نجيب ال role عشان نقدر نحمي ال api [Authorize(Roles = "Admin")]
            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            // عشان اتاكد ان التوكن اصلي مش متغير
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            // هنا ببدا اعمل التوكن
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"], // الجهه اللي اصدرت التوكن
                audience: _config["Jwt:Audience"], // الجهه اللي هتستخد التوكن
                expires: DateTime.UtcNow.AddHours(3), // تاريخ انتهاء التوكن
                claims: authClaims, // المعلومات اللي جوا التوكن
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            // تحويل التوكن ل string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            return await _dbContext.RevokedTokens.AnyAsync(T => T.Token == token);
        }

        public async Task RevokeTokenAsync(string token, DateTime expiration)
        {
            _dbContext.RevokedTokens.Add(new RevokedToken
            {
                Token = token,
                Expiration = expiration
            });

            await _dbContext.SaveChangesAsync();
        }
    }
}
