using AS4SecureGateway.WorkerService.Jobs;
using AS4SecureGateway.WorkerService.Options;
using AS4SecureGateway.WorkerService.Infrastructure;
using Quartz;

namespace AS4SecureGateway.WorkerService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAs4WorkerService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<As4WorkerOptions>()
            .Bind(configuration.GetRequiredSection("As4Worker"))
            .Validate(o => !string.IsNullOrWhiteSpace(o.SenderPartyId), "As4Worker:SenderPartyId is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.SenderRole), "As4Worker:SenderRole is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.ReceiverPartyId), "As4Worker:ReceiverPartyId is required.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.DemoPayloadXml), "As4Worker:DemoPayloadXml is required.")
            .ValidateOnStart();

        services.AddOptions<As4SchedulerOptions>()
            .Bind(configuration.GetRequiredSection("As4Scheduler"))
            .ValidateOnStart();

        var schedulerOptions = configuration
            .GetRequiredSection("As4Scheduler")
            .Get<As4SchedulerOptions>() ?? new As4SchedulerOptions();

        services.AddQuartz(quartz =>
        {
            if(schedulerOptions.EnableSendMessageJob)
            {
                var jobKey = new JobKey(nameof(SendMessageJob));

                quartz.AddJob<SendMessageJob>(options => options.WithIdentity(jobKey));

                quartz.AddTrigger(options =>
                    options
                        .ForJob(jobKey)
                        .WithIdentity($"{nameof(SendMessageJob)}Trigger")
                        .WithCronSchedule(schedulerOptions.SendMessageCron));
            }

            if (schedulerOptions.EnablePeekMessageJob)
            {
                var jobKey = new JobKey(nameof(PeekMessageJob));

                quartz.AddJob<PeekMessageJob>(options =>
                    options.WithIdentity(jobKey));

                quartz.AddTrigger(options =>
                    options
                        .ForJob(jobKey)
                        .WithIdentity($"{nameof(PeekMessageJob)}Trigger")
                        .WithCronSchedule(schedulerOptions.PeekMessageCron));
            }
        });

        services.AddQuartzHostedService(options =>
        {
           options.WaitForJobsToComplete = true; 
        });

        services.AddHostedService<DatabaseInitializerHostedService>();

        return services;
    }
}