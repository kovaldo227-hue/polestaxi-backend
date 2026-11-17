using Grpc.Core;
using Grpc.Net.Client;
using PolesTaxi.Contracts.User.V1;
using PolesTaxi.Shared.Errors;
using PolesTaxi.Shared.Types;
using Role = PolesTaxi.Shared.Types.Role;

namespace PolesTaxi.AuthService.Gateway.Grpc;

public sealed class UserGrpcGateway
{
    private readonly UserService.UserServiceClient _client;

    public UserGrpcGateway(string address)
    {
        var channel = GrpcChannel.ForAddress(address);
        _client = new UserService.UserServiceClient(channel);
    }

    public async Task<(string UserId, List<Role> Roles)> VerifyCredentials(string login, string password, CancellationToken ct)
    {
        try
        {
            var response = await _client.VerifyCredentialsAsync(
                new VerifyCredentialsRequest { Login = login, Password = password }, 
                cancellationToken: ct);
            
            var roles = response.Roles
                .Select(r => (Role)(int)r)
                .Where(r => r != Role.Unspecified)
                .ToList();
                
            return (response.UserId, roles);
        }
        catch (RpcException ex) when (ex.StatusCode is StatusCode.Unauthenticated or StatusCode.NotFound)
        {
            throw AppException.Unauthorized("invalid credentials");
        }
    }
}