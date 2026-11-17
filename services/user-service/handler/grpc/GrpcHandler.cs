using Grpc.Core;
using PolesTaxi.Contracts.User.V1;
using PolesTaxi.UserService.Repository.Mongo;

namespace PolesTaxi.UserService.Handler.Grpc;

public sealed class GrpcHandler : PolesTaxi.Contracts.User.V1.UserService.UserServiceBase
{
    private readonly UserMongoRepository _users;

    public GrpcHandler(UserMongoRepository users)
    {
        _users = users;
    }

    public override async Task<UserResponse> Register(RegisterRequest request, ServerCallContext context)
    {
        // Пока заглушка
        return new UserResponse { Id = Guid.NewGuid().ToString() };
    }
}