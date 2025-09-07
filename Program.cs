using Demodeck.Product.Api.Services;
using Demodeck.Product.Api.Models;
using Demodeck.Product.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

            // Configure CORS
            var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:8080" };
            var corsAllowCredentials = builder.Configuration.GetValue<bool>("Cors:AllowCredentials", true);
            
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsAllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                    
                    if (corsAllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                });
            });

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // JWT Configuration
            var jwtSettings = new JwtSettings
            {
                SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "DemoDeckAuthSecretKey2024ForDevelopmentOnlyNeverUseInProduction",
                Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "DemoDeckAuth",
                Audience = builder.Configuration["JwtSettings:Audience"] ?? "DemoDeckClients"
            };
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddScheme<JwtBearerOptions, TenantAwareJwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, options => {});

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
            app.UseCors();
            app.UseAuthentication();
            app.UseMiddleware<TenantContextMiddleware>();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
