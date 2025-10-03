
using API.Extensions;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Application.Interfaces.Dashboard;
using Application.Interfaces.Missions;
using Application.Interfaces.Report;
using Application.Interfaces.Submission;
using Application.Interfaces.User;
using Infrastructure.Data;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Common;
using Infrastructure.Services.Dashboard;
using Infrastructure.Services.Missions;
using Infrastructure.Services.Report;
using Infrastructure.Services.Submissions;
using Infrastructure.Services.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<LearnLinkDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IAuthResponse, AuthResponse>();
            builder.Services.AddScoped<IMissionService, MissionService>();
            builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
            builder.Services.AddScoped<IParentService, ParentService>();
            builder.Services.AddScoped<ISubmissionService, SubmissionService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddHttpContextAccessor();

            //enable jwt token
            var _authkey = builder.Configuration.GetValue<string>("JwtSettings:securitykey");
            builder.Services.AddAuthentication(item =>
            {
                item.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                item.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(item =>
            {
                item.RequireHttpsMetadata = true;
                item.SaveToken = true;
                item.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authkey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    policy
                    // Only allow these origins
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseGlobalExceptionHandling();

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
