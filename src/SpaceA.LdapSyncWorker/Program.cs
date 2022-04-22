using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SpaceA.LdapSyncWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    var section = hostContext.Configuration.GetSection(LdapOptions.CONFIG_PREFIX);
                    services.Configure<LdapOptions>(section);
                    services.Configure<SyncOptions>(hostContext.Configuration);
                    services.AddHostedService<SyncWorker>();
                });
    }
}
