using Microsoft.EntityFrameworkCore;
using WaterLevelAPI.Context;
using WaterLevelAPI.Model;

namespace WaterLevelAPI.Service
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task RegisterAsync(UserRegisterDTO userRegisterDTO)
        {
            if (userRegisterDTO is null)
                throw new ArgumentException("Dados do usuário são obrigatórios.");

            if (string.IsNullOrWhiteSpace(userRegisterDTO.Name))
                throw new ArgumentException("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(userRegisterDTO.Email))
                throw new ArgumentException("E-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(userRegisterDTO.Password) || userRegisterDTO.Password.Length < 6)
                throw new ArgumentException("Senha deve ter pelo menos 6 caracteres.");

            var normalizedEmail = userRegisterDTO.Email.Trim();

            var exists = await _context.Users
                .AnyAsync(x => x.Email.ToLower() == normalizedEmail.ToLower());

            if (exists)
                throw new ArgumentException("E-mail já cadastrado.");

            var (hash, salt) = PasswordHelper.HashPassword(userRegisterDTO.Password);

            var user = new User
            {
                Name = userRegisterDTO.Name.Trim(),
                Email = normalizedEmail,
                PasswordHash = hash,
                PasswordSalt = salt,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário cadastrado com sucesso: {Email}", user.Email);
        }

        public async Task<User> LoginAsync(UserLoginDTO userLoginDTO)
        {
            if (userLoginDTO is null)
                throw new ArgumentException("Dados de login são obrigatórios.");

            if (string.IsNullOrWhiteSpace(userLoginDTO.Email))
                throw new ArgumentException("E-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(userLoginDTO.Password))
                throw new ArgumentException("Senha é obrigatória.");

            var normalizedEmail = userLoginDTO.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail.ToLower());

            if (user is null)
                throw new ArgumentException("E-mail ou senha inválidos.");

            var isValid = PasswordHelper.VerifyPassword(userLoginDTO.Password, user.PasswordHash, user.PasswordSalt);

            if (!isValid)
                throw new ArgumentException("E-mail ou senha inválidos.");

            return user;
        }

        public async Task ForgotPasswordAsync(UserForgotPasswordDTO userForgotPasswordDTO)
        {
            if (userForgotPasswordDTO is null)
                throw new ArgumentException("Dados para recuperação são obrigatórios.");

            if (string.IsNullOrWhiteSpace(userForgotPasswordDTO.Email))
                throw new ArgumentException("E-mail é obrigatório.");

            var normalizedEmail = userForgotPasswordDTO.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail.ToLower());

            if (user is null)
                throw new ArgumentException("Nenhum usuário encontrado com este e-mail.");

            var temporaryPassword = PasswordHelper.GenerateTemporaryPassword();
            var (hash, salt) = PasswordHelper.HashPassword(temporaryPassword);

            await EmailService.SendPasswordResetEmailAsync(user.Email, temporaryPassword);

            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Senha temporária enviada para o usuário {Email}", user.Email);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordDTO changePasswordDTO)
        {
            if (changePasswordDTO is null)
                throw new ArgumentException("Dados para alteração de senha são obrigatórios.");

            if (userId <= 0)
                throw new ArgumentException("Usuário autenticado inválido.");

            if (string.IsNullOrWhiteSpace(changePasswordDTO.CurrentPassword))
                throw new ArgumentException("Senha atual é obrigatória.");

            if (string.IsNullOrWhiteSpace(changePasswordDTO.NewPassword) || changePasswordDTO.NewPassword.Length < 6)
                throw new ArgumentException("Nova senha deve ter pelo menos 6 caracteres.");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user is null)
                throw new ArgumentException("Usuário não encontrado.");

            var isCurrentPasswordValid = PasswordHelper.VerifyPassword(
                changePasswordDTO.CurrentPassword,
                user.PasswordHash,
                user.PasswordSalt);

            if (!isCurrentPasswordValid)
                throw new ArgumentException("Senha atual inválida.");

            var (hash, salt) = PasswordHelper.HashPassword(changePasswordDTO.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Senha alterada com sucesso para o usuário {Email}", user.Email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }
    }
}
