using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Student_Management_System_Data.DTOs.Account;
using Student_Management_System_Logic.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Student_Management_System_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager,ITokenService tokenService,RoleManager<IdentityRole> roleManager,ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("test-log")]
        public IActionResult TestLogging()
        {
            _logger.LogInformation("This is a test log from Serilog");
            return Ok("Log written.");
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterDTO registerDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null) return BadRequest("User already exists");

            var newUser = new  IdentityUser
            {
                Email = registerDTO.Email,
                UserName = registerDTO.Email,
                EmailConfirmed = true,
                LockoutEnabled = true,
                LockoutEnd = DateTime.MaxValue, 
            };

            var result = await _userManager.CreateAsync(newUser, registerDTO.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            var roleExist = await _roleManager.RoleExistsAsync(registerDTO.Role);
            if (!roleExist) return BadRequest("Invalid Role");

            await _userManager.AddToRoleAsync(newUser, registerDTO.Role);

            return Ok("Your account is pending admin approval.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null) return Unauthorized("Invalid email or password");

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                return Unauthorized("Your account is pending admin approval.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid email or password");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "Unknown";

            var userDto = new UserDTO
            {
                Email = user.Email,
                Role = role,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };

            return Ok(userDto);
        }

        [Authorize(Roles ="Admin")]
        [HttpGet("PendingUser")]
        public IActionResult GetPendingUser()
        {
            var users = _userManager.Users
                                    .Where(U => U.LockoutEnd != null && U.LockoutEnd > DateTime.UtcNow)
                                    .Select(U => new
                                    {
                                        U.Id,
                                        U.Email
                                    });

            return Ok(users);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Approve/{UserId}")]
        public async Task<IActionResult> ApproveUser(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null)
                return NotFound("User not found");

            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            return Ok("User approved successfully.");
        }

        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromServices] ITokenService blacklistService)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
                return Unauthorized("Token is missing");

            //بقرا التوكن عشان اقدر اجيب ال expiry date
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;

            // جاي ك Unix format
            var expUnix = jwtToken?.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

            if (long.TryParse(expUnix, out long exp))
            {
                // بحوله ل DateTime
                var expiration = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                await blacklistService.RevokeTokenAsync(token, expiration);
            }

            await _signInManager.SignOutAsync();
            return Ok("You have been logged out successfully.");
        }

    }
}
