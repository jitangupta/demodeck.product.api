using Demodeck.Product.Api.Services;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Demodeck.Product.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // JWT Configuration
            var jwtSettings = new JwtSettings
            {
                SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "DemoDeckAuthSecretKey2024ForDevelopmentOnlyNeverUseInProduction",
                Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "DemoDeckAuth",
                Audience = builder.Configuration["JwtSettings:Audience"] ?? "DemoDeckClients"
            };
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false, // Allow multiple tenants
                        ValidateAudience = false, // Allow multiple tenants
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            // Register services
            builder.Services.AddScoped<ITenantContextService, TenantContextService>();
            builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMiddleware<TenantContextMiddleware>();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
