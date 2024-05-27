using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace API.Errors
{
    public class SignalRHubFilter : IHubFilter
    {
        private readonly ILogger _logger;

        public SignalRHubFilter(ILogger<SignalRHubFilter> logger)
        {
            _logger = logger;
        }

        public async ValueTask<object?> InvokeMethodAsync(
            HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
        {
            try
            {
                return await next(invocationContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR exception calling '{0}': {1}", invocationContext.HubMethodName, ex.Message);
                return new HubException(ex.Message);
            }
        }

        public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            _logger.LogInformation($"{JsonSerializer.Serialize(context.Context.ConnectionId)} is connected");
            return next(context);
        }

        public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
        {
            _logger.LogInformation($"{JsonSerializer.Serialize(context.Context.ConnectionId)} has been offline");
            return next(context, exception);
        }
    }
}
