using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public interface IUserService
    {
        Task RegisterAsync(UserRegisterDTO userRegisterDTO);
        Task<User> LoginAsync(UserLoginDTO userLoginDTO);
        Task ForgotPasswordAsync(UserForgotPasswordDTO userForgotPasswordDTO);
        Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO);
        Task<User?> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
    }
}
