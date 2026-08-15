using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterLevelAPI.Service;

namespace WaterLevelAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;

        public UserController(IUserService service, ILogger<UserController> logger, IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO userRegisterDTO)
        {
            try
            {
                await _service.RegisterAsync(userRegisterDTO);
                return Accepted(new { Message = "Usuário cadastrado com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar usuário");
                return StatusCode(500, new { Message = "Erro interno ao cadastrar usuário." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
        {
            try
            {
                var user = await _service.LoginAsync(userLoginDTO);
                var token = JwtService.GenerateToken(user, _configuration);

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.CreatedAt,
                    Token = token
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao autenticar usuário");
                return StatusCode(500, new { Message = "Erro interno ao autenticar usuário." });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordDTO userForgotPasswordDTO)
        {
            try
            {
                await _service.ForgotPasswordAsync(userForgotPasswordDTO);
                return Accepted(new { Message = "Uma senha temporária foi enviada para o e-mail informado." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar senha do usuário");
                return StatusCode(500, new { Message = "Erro interno ao recuperar a senha." });
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { Message = "Token inválido." });

                await _service.ChangePasswordAsync(userId, changePasswordDTO);
                return Ok(new { Message = "Senha alterada com sucesso." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar senha do usuário");
                return StatusCode(500, new { Message = "Erro interno ao alterar a senha." });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return Unauthorized(new { Message = "Token inválido." });

                var user = await _service.GetByIdAsync(userId);

                if (user is null)
                    return NotFound(new { Message = "Usuário não encontrado." });

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.Role,
                    user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar perfil do usuário");
                return StatusCode(500, new { Message = "Erro interno ao buscar perfil." });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // JWT é stateless: o backend não consegue invalidar um token já emitido sem um mecanismo de blacklist.
            // O cliente deve remover o token do armazenamento local/ sessão.
            return Ok(new
            {
                Message = "Logout realizado com sucesso. Remova o token do cliente para encerrar a sessão."
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnly()
        {
            return Ok(new { Message = "Acesso liberado para administradores." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _service.GetAllAsync();

                var result = users.Select(u => new
                {
                    u.Id,
                    u.Name,
                    u.Email,
                    u.Role,
                    Status = u.IsActive ? "Ativo" : "Inativo"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar usuários");
                return StatusCode(500, new { Message = "Erro interno ao listar usuários." });
            }
        }
    }
}
