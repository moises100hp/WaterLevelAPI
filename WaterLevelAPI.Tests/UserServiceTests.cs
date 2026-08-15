using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using WaterLevelAPI;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;
using WaterLevelAPI.Service;
using Xunit;

namespace WaterLevelAPI.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task RegisterAsync_ShouldPersistTrimmedUserData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);

        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = " Maria Silva ",
            Email = " maria@example.com ",
            Password = "Senha@123"
        });

        var user = await context.Users.SingleAsync();

        Assert.Equal("Maria Silva", user.Name);
        Assert.Equal("maria@example.com", user.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.NotEqual("Senha@123", user.PasswordHash);
    }

    [Fact]
    public async Task RegisterAsync_ShouldAlwaysCreateUserRole_WhenPayloadContainsAdminRole()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);
        var request = JsonSerializer.Deserialize<UserRegisterDTO>("""
            {"name":"Maria Silva","email":"maria@example.com","password":"Senha@123","role":"Admin"}
            """, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        await service.RegisterAsync(request);

        var user = await context.Users.SingleAsync();
        Assert.Equal(UserRole.User, user.Role);
    }

    [Fact]
    public async Task RegisterAsync_ShouldRejectDuplicateEmail_IgnoringCase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);

        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "Senha@123"
        });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Outra Maria",
            Email = "MARIA@EXAMPLE.COM",
            Password = "OutraSenha@123"
        }));

        Assert.Equal("E-mail já cadastrado.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnUser_WhenCredentialsAreValid()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);

        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "Senha@123"
        });

        var user = await service.LoginAsync(new UserLoginDTO
        {
            Email = " MARIA@EXAMPLE.COM ",
            Password = "Senha@123"
        });

        Assert.Equal("Maria Silva", user.Name);
        Assert.Equal("maria@example.com", user.Email);
    }

    [Fact]
    public async Task LoginAsync_ShouldRejectInvalidPassword()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);

        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "Senha@123"
        });

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.LoginAsync(new UserLoginDTO
        {
            Email = "maria@example.com",
            Password = "SenhaErrada"
        }));

        Assert.Equal("E-mail ou senha inválidos.", exception.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenIdExists()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);

        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "Senha@123"
        });

        var storedUser = await context.Users.SingleAsync();
        var user = await service.GetByIdAsync(storedUser.Id);
        var missingUser = await service.GetByIdAsync(999);

        Assert.NotNull(user);
        Assert.Equal(storedUser.Id, user!.Id);
        Assert.Null(missingUser);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldUseAuthenticatedUserId()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        var service = new UserService(context, NullLogger<UserService>.Instance);
        await service.RegisterAsync(new UserRegisterDTO
        {
            Name = "Maria Silva",
            Email = "maria@example.com",
            Password = "Senha@123"
        });
        var user = await service.LoginAsync(new UserLoginDTO
        {
            Email = "maria@example.com",
            Password = "Senha@123"
        });

        await service.ChangePasswordAsync(user.Id, new ChangePasswordDTO
        {
            CurrentPassword = "Senha@123",
            NewPassword = "NovaSenha@123"
        });

        var authenticatedWithNewPassword = await service.LoginAsync(new UserLoginDTO
        {
            Email = "maria@example.com",
            Password = "NovaSenha@123"
        });

        Assert.Equal(user.Id, authenticatedWithNewPassword.Id);
    }

    private static AppDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
