using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using MediatR;
using System.Diagnostics;

namespace ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Behaviors
{
    public class PerformanceBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<TRequest> _logger;
        private readonly ICurrentUserService _user;

        public PerformanceBehaviour(
            ILogger<TRequest> logger,
            ICurrentUserService user)
        {
            _timer = new Stopwatch();

            _logger = logger;
            _user = user;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > 500)
            {
                var requestName = typeof(TRequest).Name;
                var userId = _user.UserId ?? string.Empty;
                var userName = _user.Username;

                _logger.LogWarning("CleanArchitecture Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@UserId} {@UserName} {@Request}",
                    requestName, elapsedMilliseconds, userId, userName, request);
            }

            return response;
        }
    }
}
