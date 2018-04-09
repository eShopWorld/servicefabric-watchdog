using System;
using System.Fabric.Health;
using System.Fabric.Query;

namespace ASFWatchdog
{
    public interface IFabricApplication
    {
        Uri ApplicationName { get; set; }
        string ApplicationTypeName { get; set; }
        string ApplicationTypeVersion { get; set; }
        HealthState HealthState { get; set; }
        ApplicationStatus ApplicationStatus { get; set; }
    }

    public class FabricApplication : IFabricApplication
    {
        public Uri ApplicationName { get; set; }
        public string ApplicationTypeName { get; set; }
        public string ApplicationTypeVersion { get; set; }
        public HealthState HealthState { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }
    }
}