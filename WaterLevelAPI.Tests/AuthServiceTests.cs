using WaterLevelAPI.Service;
using Xunit;

namespace WaterLevelAPI.Tests;

public class AuthServiceTests
{
    [Fact]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "Senha@123";

        var (hash, salt) = PasswordHelper.HashPassword(password);

        Assert.True(PasswordHelper.VerifyPassword(password, hash, salt));
    }
}
