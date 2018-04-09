using System;
using System.Fabric.Health;
using System.Fabric.Query;
using System.Linq;
using System.Threading;

namespace ASFWatchdog
{
    public class Watchdog
    {
        private static IFabricClientAdapter _fabricClientAdapter;

        public Watchdog(IFabricClientAdapter fabricClientAdapter)
        {
            _fabricClientAdapter = fabricClientAdapter;
        }

        public void InterogateAppHealth()
        {
            foreach (var application in _fabricClientAdapter.GetApplications())
            {
                if (application.ApplicationName.ToString().ToLower().Contains("watchdog"))
                    continue;

                CheckHealth(application);
            }
        }

        protected void CheckHealth(IFabricApplication application)
        {
            Console.WriteLine($"Checking service health for application: '{application.ApplicationName}");

            if (application.HealthState != HealthState.Ok 
                    && application.HealthState != HealthState.Unknown
                        && application.ApplicationStatus != ApplicationStatus.Upgrading
                            && application.ApplicationStatus != ApplicationStatus.Creating)
            {
                var services = _fabricClientAdapter.GetApplicationServices(application.ApplicationName);

                foreach (var service in services)
                {
                    if (service.HealthState != HealthState.Ok 
                            && service.HealthState != HealthState.Unknown 
                                && service.ServiceStatus != ServiceStatus.Upgrading 
                                    && service.ServiceStatus != ServiceStatus.Creating)
                    {
                        _fabricClientAdapter.RemoveService(service.ServiceName);
                    }
                }

                if(!_fabricClientAdapter.GetApplicationServices(application.ApplicationName).Any())
                    _fabricClientAdapter.RemoveApplication(application);
            }
        }
    }
}
