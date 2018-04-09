using System;
using System.Fabric.Health;
using System.Fabric.Query;

namespace ASFWatchdog
{
    public interface IFabricService
    {
        Uri ServiceName { get; set; }
        HealthState HealthState { get; set; }
        ServiceStatus ServiceStatus { get; set; }
    }

    public class FabricService : IFabricService
    {
        public Uri ServiceName { get; set; }
        public HealthState HealthState { get; set; }
        public ServiceStatus ServiceStatus { get; set; }
    }
}