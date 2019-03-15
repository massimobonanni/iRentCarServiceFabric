using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Fabric.Health;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static Task<TResponse> CallWithPolicyForTimeoutAsync<TActor, TResponse>(this TActor actor, Func<CancellationToken, Task<TResponse>> callAction,
            int maxNumberOfAttempts, Func<int, TimeSpan> waitFunc, CancellationToken token = default(CancellationToken))
            where TActor : IActor
        {
            if (actor == null)
                throw new NullReferenceException(nameof(actor));
            if (callAction == null)
                throw new ArgumentNullException(nameof(callAction));
            if (waitFunc == null)
                throw new ArgumentNullException(nameof(waitFunc));

            var retryPolicy = Policy
                            .Handle<ActorConcurrencyLockTimeoutException>()
                            .WaitAndRetryAsync(maxNumberOfAttempts, waitFunc);

            return retryPolicy.ExecuteAsync(callAction, token);
        }
    }
}