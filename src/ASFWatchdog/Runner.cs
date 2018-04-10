using System.IO;
using System.Threading;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace ASFWatchdog
{
    public class Runner
    {
        private static IContainer Container { get; set; }
        private static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<FabricClientAdapter>().As<IFabricClientAdapter>();
            Container = builder.Build();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = configBuilder.Build();

            var watchdog = new Watchdog(Container.Resolve<FabricClientAdapter>());

            CancellationTokenSource cts = new CancellationTokenSource();

            while (!cts.IsCancellationRequested)
            {
                watchdog.InterogateAppHealth();

                Thread.Sleep(int.Parse(Configuration["schedule"]) * 1000);
            }
        }
    }
}