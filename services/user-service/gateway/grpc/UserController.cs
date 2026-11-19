using Microsoft.AspNetCore.Mvc;
using PolesTaxi.UserService.entity;        // <--- Ссылка на User
using PolesTaxi.UserService.repository.mongo; // <--- Ссылка на репозиторий

namespace PolesTaxi.UserService.gateway.grpc; // <--- Namespace папки

[ApiController]
[Route("v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _repository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Пользователь с таким email уже существует" });
        }

        var user = new User
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            PasswordHash = request.Password, 
            Role = request.Role ?? "ROLE_USER"
        };

        await _repository.CreateAsync(user);

        return Ok(new { message = "Пользователь успешно зарегистрирован", userId = user.Id });
    }
}

public class RegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Role { get; set; }
}