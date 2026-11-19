using Grpc.Core;
using PolesTaxi.Shared.Errors;
namespace PolesTaxi.Shared.Hosting;
public static class CallContext
{
    public static string RequireUserId(ServerCallContext context)
    {
        var userId = context.RequestHeaders.FirstOrDefault(x => x.Key == "x-user-id")?.Value;
        if (string.IsNullOrEmpty(userId)) throw AppException.Unauthorized("x-user-id header is missing");
        return userId;
    }
}
