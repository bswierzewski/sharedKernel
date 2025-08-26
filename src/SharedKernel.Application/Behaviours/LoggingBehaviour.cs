using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using SharedKernel.Application.Interfaces;

namespace SharedKernel.Application.Behaviours;

public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest> where TRequest : notnull
{
    private readonly ILogger _logger;
    private readonly IUser? _user;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser? user = null)
    {
        _logger = logger;
        _user = user;
    }

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user?.Id.ToString() ?? string.Empty;

        _logger.LogInformation("Request: {Name} {@UserId} {@Request}", requestName, userId, request);

        await Task.CompletedTask;
    }
}