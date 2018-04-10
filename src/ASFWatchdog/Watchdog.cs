using System;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Linq;

namespace ASFWatchdog
{
    using System.Threading.Tasks;

    public class Watchdog
    {
        private static FabricClient _fabricClient;

        public Watchdog()
        {
            try
            {
                _fabricClient = new FabricClient();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to connect to the fabric, exception: {ex.Message}");
                throw;
            }
        }

        public async Task InterrogateAppHealth()
        {
            foreach (Application application in await _fabricClient.QueryManager.GetApplicationListAsync())
            {
                if (application.ApplicationName.ToString().ToLower().Contains("watchdog"))
                    continue;

                await CheckHealth(application);
            }
        }

        private async Task CheckHealth(Application application)
        {
            Console.WriteLine($"Checking service health for application: '{application.ApplicationName}");

            if (application.HealthState != HealthState.Ok && 
                application.HealthState != HealthState.Unknown && 
                application.ApplicationStatus != ApplicationStatus.Upgrading && 
                application.ApplicationStatus != ApplicationStatus.Creating)
            {
                foreach (var service in await _fabricClient.QueryManager.GetServiceListAsync(application.ApplicationName))
                {
                    if (service.HealthState != HealthState.Ok && 
                        service.HealthState != HealthState.Unknown && 
                        service.ServiceStatus != ServiceStatus.Upgrading && 
                        service.ServiceStatus != ServiceStatus.Creating)
                    {
                        try
                        {
                            Console.WriteLine($"Removing unhealthy service:'{service.ServiceName}");
                            await _fabricClient.ServiceManager.DeleteServiceAsync(new DeleteServiceDescription(service.ServiceName));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to delete the service: '{service.ServiceName}', exception: {ex.Message}");
                            throw;
                        }
                    }
                }

                var appServices = await _fabricClient.QueryManager.GetServiceListAsync(application.ApplicationName);

                if (!appServices.Any())
                    await RemoveApplication(application);
            }
        }

        private async Task RemoveApplication(Application application)
        {
            var services = _fabricClient.QueryManager.GetServiceListAsync(application.ApplicationName).Result;

            if (services.Count == 0)
            {
                Console.WriteLine($"No services on the application, removing application:'{application.ApplicationName}");

                try
                {
                    await _fabricClient.ApplicationManager.DeleteApplicationAsync(new DeleteApplicationDescription(application.ApplicationName));
                    await _fabricClient.ApplicationManager.UnprovisionApplicationAsync(
                        new UnprovisionApplicationTypeDescription(application.ApplicationTypeName, application.ApplicationTypeVersion));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to delete/unprovision the application: '{application.ApplicationName}', exception: {ex.Message}");
                    throw;
                }
            }
        }
    }
}