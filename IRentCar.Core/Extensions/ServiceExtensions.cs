using System;
using System.Collections.Generic;
using System.Fabric.Health;
using System.Text;

namespace Microsoft.ServiceFabric.Services.Runtime
{
    public static class ServiceExtensions
    {
        public static void ReportHealthInformation(this StatefulServiceBase service, string property, string description,
                HealthState state, int secondsToLive = 0)
        {
            if (service == null)
                throw new NullReferenceException(nameof(service));

            HealthInformation healthInformation = new HealthInformation(service.Context.ServiceName.ToString()
                , property, state);
            healthInformation.Description = description;
            if (secondsToLive > 0) healthInformation.TimeToLive = TimeSpan.FromSeconds(secondsToLive);
            healthInformation.RemoveWhenExpired = true;
            try
            {
                var activationContext = service.Context.CodePackageActivationContext;
                activationContext.ReportApplicationHealth(healthInformation);
            }
            catch { }
        }

        public static void ReportHealthInformation(this StatelessService service, string property, string description,
                HealthState state, int secondsToLive = 0)
        {
            if (service == null)
                throw new NullReferenceException(nameof(service));

            HealthInformation healthInformation = new HealthInformation(service.Context.ServiceName.ToString()
                , property, state);
            healthInformation.Description = description;
            if (secondsToLive > 0) healthInformation.TimeToLive = TimeSpan.FromSeconds(secondsToLive);
            healthInformation.RemoveWhenExpired = true;
            try
            {
                var activationContext = service.Context.CodePackageActivationContext;
                activationContext.ReportApplicationHealth(healthInformation);
            }
            catch { }
        }
    }
}