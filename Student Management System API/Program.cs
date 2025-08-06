
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Student_Management_System_API.DataSeed;
using Student_Management_System_API.Middelware;
using Student_Management_System_API.Validators.Course;
using Student_Management_System_API.Validators.Enrollment;
using Student_Management_System_API.Validators.Student;
using Student_Management_System_API.Validators.Teacher;
using Student_Management_System_Data.Data;
using Student_Management_System_Data.DTOs.Enrollment;
using Student_Management_System_Data.Mapping;
using Student_Management_System_Logic.Interfaces;
using Student_Management_System_Logic.Repositories;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Microsoft.Data.SqlClient;

namespace Student_Management_System_API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
               



            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            #region Add Database Service

            builder.Services.AddDbContext<StudentDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<StudentDbContext>()
                    .AddDefaultTokenProviders();

            #endregion

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddAutoMapper(M => M.AddProfile(new StudentProfile()));
            builder.Services.AddAutoMapper(M => M.AddProfile(new TeacherProfile()));
            builder.Services.AddAutoMapper(M => M.AddProfile(new CourseProfile()));
            builder.Services.AddAutoMapper(M => M.AddProfile(new EnrollmentProfile()));
            builder.Services.AddValidatorsFromAssembly(typeof(StudentCreateDTOValidator).Assembly);
            


            #region JWT
            // œÂ ⁄‘«‰ « «ﬂœ „‰ «· Êﬂ‰
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"]))
                };
            });
            #endregion

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("Logs/log.text")
                .CreateLogger();

            builder.Host.UseSerilog();

            var app = builder.Build();

            app.UseMiddleware<TokenValidationMiddelware>();
            app.UseMiddleware<ExceptionHandlingMiddelware>();


            #region Update Database
            using var scope = app.Services.CreateScope(); // ﬂ· «· services «··Ì ‘€«·Â scope

            var service = scope.ServiceProvider; // Ã»  «· services

            var context = service.GetRequiredService<StudentDbContext>(); // Ã»  «· service «··Ì ⁄«Ì“«Â«

            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            try
            {
                await context.Database.MigrateAsync();
                await IdentitySeed.SeedAsync(service);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "There Are Problems During Apply Migrations !!");
            }

            #endregion

           

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
