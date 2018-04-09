using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Fabric.Health;
using System.Linq;

namespace ASFWatchdog
{
    public interface IFabricClientAdapter
    {
        IEnumerable<IFabricApplication> GetApplications();
        IEnumerable<IFabricService> GetApplicationServices(Uri applicationName);
        HealthState GetServiceHealth(Uri applicationName, Uri serviceName);
        void RemoveService(Uri serviceName);
        void RemoveApplication(IFabricApplication application);
    }

    public class FabricClientAdapter : IFabricClientAdapter
    {
        private readonly FabricClient _fabricClient;

        public FabricClientAdapter()
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

        public IEnumerable<IFabricApplication> GetApplications()
        {
            var apps = new List<IFabricApplication>();

            var appList = _fabricClient.QueryManager.GetApplicationListAsync().Result.ToList();

            foreach (var application in appList)
                apps.Add(new FabricApplication
                {
                    ApplicationName = application.ApplicationName,
                    ApplicationStatus = application.ApplicationStatus,
                    HealthState = application.HealthState,
                    ApplicationTypeName = application.ApplicationTypeName,
                    ApplicationTypeVersion = application.ApplicationTypeVersion
                });

            return apps;
        }

        public IEnumerable<IFabricService> GetApplicationServices(Uri applicationName)
        {
            var services = new List<FabricService>();

            var serviceList = _fabricClient.QueryManager.GetServiceListAsync(applicationName).Result.ToList();

            foreach (var service in serviceList)
            {
                services.Add(new FabricService
                {
                    ServiceName = service.ServiceName,
                    HealthState = service.HealthState,
                    ServiceStatus = service.ServiceStatus
                });
            }

            return new List<FabricService>();
        }

        public HealthState GetServiceHealth(Uri applicationName, Uri serviceName)
        {
            return _fabricClient.QueryManager.GetServiceListAsync(applicationName, serviceName).Result.First()
                .HealthState;
        }

        public void RemoveService(Uri serviceName)
        {
            Console.WriteLine($"Removing unhealthy service:'{serviceName}");

            try
            {
                _fabricClient.ServiceManager.DeleteServiceAsync(new DeleteServiceDescription(serviceName)).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to delete the service: '{serviceName}', exception: {ex.Message}");
                throw;
            }
        }

        public void RemoveApplication(IFabricApplication application)
        {
            var services = _fabricClient.QueryManager.GetServiceListAsync(application.ApplicationName).Result;

            if (services.Count == 0)
            {
                Console.WriteLine(
                    $"No services on the application, removing application:'{application.ApplicationName}");

                try
                {
                    _fabricClient.ApplicationManager
                        .DeleteApplicationAsync(new DeleteApplicationDescription(application.ApplicationName)).Wait();
                    _fabricClient.ApplicationManager.UnprovisionApplicationAsync(
                        new UnprovisionApplicationTypeDescription(application.ApplicationTypeName,
                            application.ApplicationTypeVersion)).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Unable to delete/unprovision the application: '{application.ApplicationName}', exception: {ex.Message}");
                    throw;
                }
            }
        }
    }
}