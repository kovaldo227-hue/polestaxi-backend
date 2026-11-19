using Grpc.Core;
using PolesTaxi.Contracts.User.V1;
using PolesTaxi.UserService.Service;
using Role = PolesTaxi.Shared.Types.Role; // Явно указываем, какой Role использовать

namespace PolesTaxi.UserService.Handler.Grpc;

public sealed class GrpcHandler : PolesTaxi.Contracts.User.V1.UserService.UserServiceBase
{
    private readonly UserAppService _service;

    public GrpcHandler(UserAppService service)
    {
        _service = service;
    }

    public override async Task<UserResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        var role = (Role)(int)request.Role;
        var user = await _service.Register(request.Name, request.Phone, request.Email, request.Password, role, context.CancellationToken);
        return new UserResponse { Id = user.Id, Name = user.Name, Phone = user.Phone, Email = user.Email };
    }
}