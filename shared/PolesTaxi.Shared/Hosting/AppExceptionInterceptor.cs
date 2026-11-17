using Grpc.Core;
using Grpc.Core.Interceptors;
using PolesTaxi.Shared.Errors;

namespace PolesTaxi.Shared.Hosting;

public sealed class AppExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try { return await continuation(request, context); }
        catch (AppException ex)
        {
            var statusCode = ex.Code switch
            {
                ErrorCodes.Validation => StatusCode.InvalidArgument,
                ErrorCodes.Unauthorized => StatusCode.Unauthenticated,
                ErrorCodes.Forbidden => StatusCode.PermissionDenied,
                ErrorCodes.NotFound => StatusCode.NotFound,
                ErrorCodes.Conflict => StatusCode.AlreadyExists,
                _ => StatusCode.Internal
            };
            throw new RpcException(new Status(statusCode, ex.Message));
        }
    }
}