using Demodeck.Product.Api.Models;

namespace Demodeck.Product.Api.Services
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public InMemoryUserRepository()
        {
            SeedUsers();
        }

        private void SeedUsers()
        {
            var users = new List<User>();
            int userId = 1;

            // ACME Corporation Users (tnt_acme001)
            users.AddRange(new[]
            {
                new User
                {
                    Id = userId++,
                    Username = "john.doe",
                    Email = "john.doe@acme.com",
                    TenantId = "tnt_acme001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-180)
                },
                new User
                {
                    Id = userId++,
                    Username = "sarah.johnson",
                    Email = "sarah.johnson@acme.com",
                    TenantId = "tnt_acme001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-150)
                },
                new User
                {
                    Id = userId++,
                    Username = "mike.wilson",
                    Email = "mike.wilson@acme.com",
                    TenantId = "tnt_acme001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-120)
                },
                new User
                {
                    Id = userId++,
                    Username = "lisa.brown",
                    Email = "lisa.brown@acme.com",
                    TenantId = "tnt_acme001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-90)
                }
            });

            // GlobalX Industries Users (tnt_globalx001)
            users.AddRange(new[]
            {
                new User
                {
                    Id = userId++,
                    Username = "alex.smith",
                    Email = "alex.smith@globalx.com",
                    TenantId = "tnt_globalx001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-160)
                },
                new User
                {
                    Id = userId++,
                    Username = "emma.davis",
                    Email = "emma.davis@globalx.com",
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-140)
                },
                new User
                {
                    Id = userId++,
                    Username = "james.taylor",
                    Email = "james.taylor@globalx.com",
                    TenantId = "tnt_globalx001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-100)
                },
                new User
                {
                    Id = userId++,
                    Username = "maria.garcia",
                    Email = "maria.garcia@globalx.com",
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-80)
                },
                new User
                {
                    Id = userId++,
                    Username = "robert.lee",
                    Email = "robert.lee@globalx.com",
                    TenantId = "tnt_globalx001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                }
            });

            // Initech LLC Users (tnt_initech001)
            users.AddRange(new[]
            {
                new User
                {
                    Id = userId++,
                    Username = "peter.gibbons",
                    Email = "peter.gibbons@initech.com",
                    TenantId = "tnt_initech001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-75)
                },
                new User
                {
                    Id = userId++,
                    Username = "samir.nagheenanajar",
                    Email = "samir.n@initech.com",
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-65)
                },
                new User
                {
                    Id = userId++,
                    Username = "michael.bolton",
                    Email = "michael.bolton@initech.com",
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-55)
                },
                new User
                {
                    Id = userId++,
                    Username = "milton.waddams",
                    Email = "milton.waddams@initech.com",
                    TenantId = "tnt_initech001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                }
            });

            // Umbrella Corp Users (tnt_umbrella001)
            users.AddRange(new[]
            {
                new User
                {
                    Id = userId++,
                    Username = "albert.wesker",
                    Email = "albert.wesker@umbrella.com",
                    TenantId = "tnt_umbrella001",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-35)
                },
                new User
                {
                    Id = userId++,
                    Username = "ada.wong",
                    Email = "ada.wong@umbrella.com",
                    TenantId = "tnt_umbrella001",
                    Role = "Manager",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Id = userId++,
                    Username = "william.birkin",
                    Email = "william.birkin@umbrella.com",
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new User
                {
                    Id = userId++,
                    Username = "annette.birkin",
                    Email = "annette.birkin@umbrella.com",
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new User
                {
                    Id = userId++,
                    Username = "spencer.parks",
                    Email = "spencer.parks@umbrella.com",
                    TenantId = "tnt_umbrella001",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            });

            _users.AddRange(users);
        }

        public async Task<List<User>> GetUsersByTenantAsync(string tenantId)
        {
            return _users.Where(u => u.TenantId == tenantId && u.IsActive).ToList();
        }
    }
}