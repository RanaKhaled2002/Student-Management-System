using Student_Management_System_Logic.Interfaces;

namespace Student_Management_System_API.Middelware
{
    public class TokenValidationMiddelware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddelware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITokenService blacklistService)
        {
            // بجيب التوكن من الهيدر
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var isRevoked = await blacklistService.IsTokenRevokedAsync(token);
                if (isRevoked)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
