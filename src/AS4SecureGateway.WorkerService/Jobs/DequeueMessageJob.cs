using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using AS4SecureGateway.WorkerService.Options;
using Microsoft.Extensions.Options;
using Quartz;

namespace AS4SecureGateway.WorkerService.Jobs;

[DisallowConcurrentExecution]
public sealed class DequeueMessageJob : IJob
{
    private readonly ILogger<DequeueMessageJob> _logger;
    private readonly DispatchAs4MessageHandler _handler;
    private readonly As4WorkerOptions _options;

    public DequeueMessageJob(
        ILogger<DequeueMessageJob> logger,
        DispatchAs4MessageHandler handler,
        IOptions<As4WorkerOptions> options)
    {
        _logger = logger;
        _handler = handler;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (string.IsNullOrWhiteSpace(_options.DocumentReferenceNumber))
        {
            _logger.LogWarning(
                "[DequeueMessage] Skipped. DocumentReferenceNumber is not configured.");

            return;
        }

        _logger.LogInformation("[DequeueMessage] Job started.");

        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.DequeueMessage,

            SenderPartyId = _options.SenderPartyId,
            SenderRole = _options.SenderRole,
            ReceiverPartyId = _options.ReceiverPartyId,

            DocumentReferenceNumber = _options.DocumentReferenceNumber,
            SecurityOptions = _options.ToSecurityOptions()
        };

        var result = await _handler.HandleAsync(command, context.CancellationToken);

        _logger.LogInformation(
            "[DequeueMessage] Job finished. HTTP = {StatusCode}, Success = {Success}",
            (int)result.StatusCode,
            result.IsSuccessStatusCode);
    }
}