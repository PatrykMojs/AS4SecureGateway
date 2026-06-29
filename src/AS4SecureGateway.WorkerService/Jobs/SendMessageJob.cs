using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using AS4SecureGateway.WorkerService.Options;
using Microsoft.Extensions.Options;
using Quartz;

namespace AS4SecureGateway.WorkerService.Jobs;

[DisallowConcurrentExecution]
public sealed class SendMessageJob : IJob
{
    private readonly ILogger<SendMessageJob> _logger;
    private readonly DispatchAs4MessageHandler _handler;
    private readonly As4WorkerOptions _options;

    public SendMessageJob( 
        ILogger<SendMessageJob> logger,
        DispatchAs4MessageHandler handler,
        IOptions<As4WorkerOptions> options)
    {
        _logger = logger;
        _handler = handler;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[SendMessage] Job started.");

        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.SendMessage,

            SenderPartyId = _options.SenderPartyId,
            SenderRole = _options.SenderRole,
            ReceiverPartyId = _options.ReceiverPartyId,

            PayloadXml = _options.DemoPayloadXml,
            TestScenario = _options.TestScenario,

            SecurityOptions = _options.ToSecurityOptions()
        };

        var result = await _handler.HandleAsync(command, context.CancellationToken);

        if (result.IsSuccessStatusCode && !result.ParsedResponse.HasFault)
        {
            _logger.LogInformation(
                "[SendMessage] Job finished successfully. HTTP = {StatusCode}, Status = {Status}, MessageId = {MessageId}, DocumentId = {DocumentId}",
                (int)result.StatusCode,
                result.ParsedResponse.Status,
                result.ParsedResponse.MessageId,
                result.ParsedResponse.DocumentId);
        }
        else
        {
            _logger.LogWarning(
                "[SendMessage] Job finished with error. HTTP = {StatusCode}, ErrorCode = {ErrorCode}, Description = {Description}, FaultReason = {FaultReason}",
                (int)result.StatusCode,
                result.ParsedResponse.ErrorCode,
                result.ParsedResponse.ErrorDescription,
                result.ParsedResponse.FaultReason);
        }
    }
}