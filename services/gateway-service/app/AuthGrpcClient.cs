using Grpc.Net.Client;
using PolesTaxi.Contracts.Auth.V1;

namespace PolesTaxi.GatewayService.App;

public sealed class AuthGrpcClient
{
    private readonly AuthService.AuthServiceClient _client;

    public AuthGrpcClient(string address)
    {
        var channel = GrpcChannel.ForAddress(address);
        _client = new AuthService.AuthServiceClient(channel);
    }

    public async Task<ValidateTokenResponse?> ValidateToken(string token, CancellationToken ct)
    {
        try
        {
            return await _client.ValidateTokenAsync(new ValidateTokenRequest { AccessToken = token }, cancellationToken: ct);
        }
        catch
        {
            return null;
        }
    }
}