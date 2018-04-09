using System;
using System.Collections.Generic;
using System.Fabric.Health;
using DevOpsFlex.Tests.Core;
using Moq;
using Xunit;

namespace ASFWatchdog.Tests
{
    public class ASFWatchdogTests
    {
        [Fact, IsUnit]
        public void No_Applications_DoNothing()
        {
            //arrange
            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<FabricApplication>()); 

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
        }

        [Fact, IsUnit]
        public void Watchdog_Application_DoNothing()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/AwaTchDog_App"));

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            appMock.VerifyAll();
        }

        [Fact, IsUnit]
        public void Healthy_Application_DoNothing()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/HealthyApp"));
            appMock.SetupGet(x => x.HealthState).Returns(HealthState.Ok);

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            appMock.VerifyAll();
        }

        [Fact, IsUnit]
        public void Unknown_Application_DoNothing()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/UnknownApp"));
            appMock.SetupGet(x => x.HealthState).Returns(HealthState.Unknown);

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            appMock.VerifyAll(); 
        }

        [Fact, IsUnit]
        public void UnHealhty_Application_Remove()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/TestApp"));
            appMock.SetupGet(x => x.HealthState).Returns(HealthState.Error);

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            fabricClientAdaptorMock.Verify(x => x.RemoveApplication(appMock.Object));
        }

        [Fact, IsUnit]
        public void UnHealhty_Service_Remove()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/TestApp"));
            appMock.SetupGet(x => x.HealthState).Returns(HealthState.Error);

            var serviceMock = new Mock<IFabricService>();
            serviceMock.SetupGet(x => x.ServiceName).Returns(new Uri("fabric:/TestService"));
            serviceMock.SetupGet(x => x.HealthState).Returns(HealthState.Error);

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>();
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });
            fabricClientAdaptorMock.SetupSequence(x => x.GetApplicationServices(new Uri("fabric:/TestApp")))
                    .Returns(new List<IFabricService>() {serviceMock.Object})
                    .Returns(new List<IFabricService>());
            fabricClientAdaptorMock.Setup(x => x.GetServiceHealth(new Uri("fabric:/TestApp"), new Uri("fabric:/TestService"))).Returns(HealthState.Error);

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            fabricClientAdaptorMock.Verify(x => x.RemoveService(new Uri("fabric:/TestService")));
            fabricClientAdaptorMock.Verify(x => x.RemoveApplication(appMock.Object));
        }

        [Fact, IsUnit]
        public void UnHealhty_Service_ThatChangesToHealthy_DoNothing()
        {
            //arrange
            var appMock = new Mock<IFabricApplication>();
            appMock.SetupGet(x => x.ApplicationName).Returns(new Uri("fabric:/TestApp"));
            appMock.SetupGet(x => x.HealthState).Returns(HealthState.Error);

            var serviceMock = new Mock<IFabricService>();
            serviceMock.SetupGet(x => x.ServiceName).Returns(new Uri("fabric:/TestService"));
            serviceMock.SetupGet(x => x.HealthState).Returns(HealthState.Error);

            var fabricClientAdaptorMock = new Mock<IFabricClientAdapter>(MockBehavior.Strict);
            fabricClientAdaptorMock.Setup(x => x.GetApplications()).Returns(new List<IFabricApplication>() { appMock.Object });
            fabricClientAdaptorMock.Setup(x => x.GetApplicationServices(new Uri("fabric:/TestApp"))).Returns(new List<IFabricService>() { serviceMock.Object });
            fabricClientAdaptorMock.Setup(x => x.GetServiceHealth(new Uri("fabric:/TestApp"), new Uri("fabric:/TestService"))).Returns(HealthState.Ok);

            var watchdog = new Watchdog(fabricClientAdaptorMock.Object);

            //act
            watchdog.InterogateAppHealth();

            //assert
            fabricClientAdaptorMock.VerifyAll();
        }
    }
}