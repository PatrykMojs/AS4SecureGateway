using AS4SecureGateway.Application.Messaging;
using AS4SecureGateway.Application.UseCases.DispatchAs4Message;
using AS4SecureGateway.WorkerService.Options;
using AS4SecureGateway.WorkerService.Utilities;
using Microsoft.Extensions.Options;
using Quartz;

namespace AS4SecureGateway.WorkerService.Jobs;

[DisallowConcurrentExecution]
public sealed class PeekMessageJob : IJob
{
    private readonly ILogger<PeekMessageJob> _logger;
    private readonly DispatchAs4MessageHandler _handler;
    private readonly As4WorkerOptions _options;

    public PeekMessageJob(ILogger<PeekMessageJob> logger, DispatchAs4MessageHandler handler, IOptions<As4WorkerOptions> options)
    {
        _logger = logger;
        _handler = handler;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("[PeekMessage] Job started.");

        var command = new DispatchAs4MessageCommand
        {
            ActionType = As4ActionType.PeekMessage,

            SenderPartyId = _options.SenderPartyId,
            SenderRole = _options.SenderRole,
            ReceiverPartyId = _options.ReceiverPartyId,

            PeekMessageDomain = _options.PeekMessageDomain,
            TestScenario = _options.TestScenario,
            
            SecurityOptions = _options.ToSecurityOptions()
        };

        var result = await _handler.HandleAsync(command, context.CancellationToken);

        _logger.LogInformation(
            """
            ================= AS4 SERVER RESPONSE XML =================

            HTTP Status: {StatusCode}

            {ResponseXml}

            ============================================================
            """,
            (int)result.StatusCode,
            XmlLogFormatter.Format(result.ResponseBody));

        if (result.IsSuccessStatusCode && !result.ParsedResponse.HasFault)
        {
            _logger.LogInformation(
                "[PeekMessage] Job finished successfully. HTTP = {StatusCode}, Status = {Status}, MessageId = {MessageId}",
                (int)result.StatusCode,
                result.ParsedResponse.Status,
                result.ParsedResponse.MessageId);
        }
        else
        {
            _logger.LogWarning(
                "[PeekMessage] Job finished with error. HTTP = {StatusCode}, ErrorCode = {ErrorCode}, Description = {Description}, FaultReason = {FaultReason}",
                (int)result.StatusCode,
                result.ParsedResponse.ErrorCode,
                result.ParsedResponse.ErrorDescription,
                result.ParsedResponse.FaultReason);
        }
    }
}