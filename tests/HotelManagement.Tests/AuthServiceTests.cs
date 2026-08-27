using HotelManagement.Application.DTOs.Auth;
using HotelManagement.Application.Services;
using HotelManagement.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace HotelManagement.Tests;

public class AuthServiceTests
{
    private static (TestDbContext Db, AuthService Auth) CreateSut()
    {
        var db = new TestDbContext();
        var userRepo = new UserRepository(db.Context);

        var myConfig = new Dictionary<string, string?>
        {
            { "Jwt:Key", "HotelManagement_Super_Secret_Key_At_Least_32_Chars!" },
            { "Jwt:Issuer", "HotelManagementAPI" },
            { "Jwt:Audience", "HotelManagementClient" }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfig)
            .Build();

        return (db, new AuthService(userRepo, config));
    }

    [Fact]
    public async Task Register_CreatesUser_AndReturnsToken()
    {
        var (db, auth) = CreateSut();
        using (db)
        {
            var response = await auth.RegisterAsync(new RegisterDTO
            {
                Username = "admin",
                Email = "admin@hotel.com",
                Password = "Password123!",
                Role = "Admin"
            });

            Assert.NotNull(response.Token);
            Assert.NotEmpty(response.Token);
            Assert.Equal("admin", response.Username);
            Assert.Equal("admin@hotel.com", response.Email);
            Assert.Equal("Admin", response.Role);
        }
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var (db, auth) = CreateSut();
        using (db)
        {
            await auth.RegisterAsync(new RegisterDTO
            {
                Username = "user1",
                Email = "test@hotel.com",
                Password = "Password123!"
            });

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => auth.RegisterAsync(new RegisterDTO
                {
                    Username = "user2",
                    Email = "test@hotel.com",
                    Password = "Password123!"
                }));
        }
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtToken()
    {
        var (db, auth) = CreateSut();
        using (db)
        {
            await auth.RegisterAsync(new RegisterDTO
            {
                Username = "marwan",
                Email = "marwan@hotel.com",
                Password = "SecretPassword123!"
            });

            var response = await auth.LoginAsync(new LoginDTO
            {
                Email = "marwan@hotel.com",
                Password = "SecretPassword123!"
            });

            Assert.NotNull(response.Token);
            Assert.NotEmpty(response.Token);
            Assert.Equal("marwan", response.Username);
        }
    }

    [Fact]
    public async Task Login_InvalidPassword_IsRejected()
    {
        var (db, auth) = CreateSut();
        using (db)
        {
            await auth.RegisterAsync(new RegisterDTO
            {
                Username = "marwan",
                Email = "marwan@hotel.com",
                Password = "SecretPassword123!"
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => auth.LoginAsync(new LoginDTO
                {
                    Email = "marwan@hotel.com",
                    Password = "WrongPassword!"
                }));
        }
    }
}
