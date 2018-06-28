using System;
using System.Collections.Generic;
using System.Fabric.Health;
using System.Text;

namespace Microsoft.ServiceFabric.Actors.Runtime
{
    public static class ActorExtensions
    {
        public static void ReportHealthInformation(this ActorBase actor, string property, string description,
                HealthState state, int secondsToLive = 0)
        {
            if (actor == null)
                throw new NullReferenceException(nameof(actor));

            HealthInformation healthInformation = new HealthInformation(actor.Id.ToString(), property, state);
            healthInformation.Description = description;
            if (secondsToLive > 0) healthInformation.TimeToLive = TimeSpan.FromSeconds(secondsToLive);
            healthInformation.RemoveWhenExpired = true;
            try
            {
                var activationContext = actor.ActorService.Context.CodePackageActivationContext;
                activationContext.ReportApplicationHealth(healthInformation);
            }
            catch { }
        }
    }
}