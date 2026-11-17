using Grpc.Core;
using PolesTaxi.Contracts.Auth.V1;

namespace PolesTaxi.AuthService.Handler.Grpc;

public sealed class GrpcHandler : PolesTaxi.Contracts.Auth.V1.AuthService.AuthServiceBase
{
    public override Task<TokenResponse> Login(LoginRequest request, ServerCallContext context)
    {
        return Task.FromResult(new TokenResponse { AccessToken = "test", RefreshToken = "test" });
    }
}