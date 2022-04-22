using SpaceA.WebApi.HostedServices;
using SpaceA.WebApi.Jobs;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExts
    {
        public static void AddJobSchedule(this IServiceCollection services)
        {
            services.AddSingleton<IJobFactory, SingletonJobFactory>()
                .AddSingleton<ISchedulerFactory, StdSchedulerFactory>()
                .AddSingleton<RemainingWorkAccountingJob>()
                .AddSingleton(new JobSchedule(typeof(RemainingWorkAccountingJob), "0 0 3 * * ?"));
            //.AddSingleton(new JobSchedule(typeof(RemainingWorkAccountingJob), "0/30 * * * * ?"));
            services.AddHostedService<QuartzHostedService>();
        }
    }
}