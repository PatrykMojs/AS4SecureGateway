namespace AS4SecureGateway.WorkerService.Options;

public sealed class As4SchedulerOptions
{
    public bool EnableSendMessageJob { get; init; } = true;
    public string SendMessageCron { get; init; } = "0/30 * * * * ?";

    public bool EnablePeekMessageJob { get; init; } = false;
    public string PeekMessageCron { get; init; } = "0 0/1 * * * ?";

    public bool EnableDequeueMessageJob { get; init; } = false;
    public string DequeueMessageCron { get; init; } = "0 0/2 * * * ?";
}