using PolesTaxi.UserService.entity;          // <--- Ссылка на User
using PolesTaxi.UserService.repository.mongo; // <--- Ссылка на репозиторий

namespace PolesTaxi.UserService.service;

public interface IUserAppService
{
    Task<User?> GetUserByEmailAsync(string email);
    Task CreateUserAsync(User user);
}

public class UserAppService : IUserAppService
{
    private readonly IUserRepository _repository;

    public UserAppService(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _repository.GetByEmailAsync(email);
    }

    public async Task CreateUserAsync(User user)
    {
        await _repository.CreateAsync(user);
    }
}