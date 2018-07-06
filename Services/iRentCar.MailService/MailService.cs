using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Implementations;
using iRentCar.Core.Interfaces;
using iRentCar.MailService.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace iRentCar.MailService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MailService : StatefulService, IMailService
    {
        public MailService(StatefulServiceContext context, IActorFactory actorFactory = null)
            : base(context)
        {
            if (actorFactory == null)
                this.actorFactory = new ReliableFactory();
            else
                this.actorFactory = actorFactory;
        }

        public MailService(StatefulServiceContext context, IReliableStateManagerReplica stateManager, IActorFactory actorFactory = null)
            : base(context, stateManager)
        {
            if (actorFactory == null)
                this.actorFactory = new ReliableFactory();
            else
                this.actorFactory = actorFactory;
        }

        private readonly IActorFactory actorFactory;

        internal const string MailQueueName = "MailQueue";
        private IReliableQueue<MailData> mailQueue;

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            mailQueue = await this.StateManager.GetOrAddAsync<IReliableQueue<MailData>>(MailQueueName);

            ConfigureService();

            List<Task> taskList = new List<Task>();

            taskList.Add(SendMailTaskCode(cancellationToken));

            var taskResult = await Task.WhenAny(taskList);

            if (!cancellationToken.IsCancellationRequested)
            {
                if (taskResult.IsFaulted)
                {
                    throw taskResult.Exception;
                }
                else
                {
                    throw new InvalidOperationException("One or more tasks completed in unexpected ways");
                }
            }
        }

        #region Configuration methods

        internal TimeSpan DelayBetweenMailSend = TimeSpan.FromSeconds(30);

        private void ConfigureService()
        {
            var config = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            this.ReadSettings(config.Settings);
            this.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent +=
                this.CodePackageActivationContext_ConfigurationPackageModifiedEvent;
        }

        private void CodePackageActivationContext_ConfigurationPackageModifiedEvent(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            this.ReadSettings(e.NewPackage.Settings, true);
        }

        /// <summary>
        /// Reads the settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="updated">if set to <c>true</c> [updated].</param>
        private void ReadSettings(ConfigurationSettings settings, bool updated = false)
        {
            var mailServiceSection = settings.Sections["MailService"];

            TimeSpan delayBetweenMailSend;
            if (mailServiceSection.Parameters.Contains("DelayBetweenMailSend") &&
                TimeSpan.TryParse(mailServiceSection.Parameters["DelayBetweenMailSend"].Value, out delayBetweenMailSend))
            {
                this.DelayBetweenMailSend = delayBetweenMailSend;
            }



        }
        #endregion

        #region [ Tasks ]
        private async Task SendMailTaskCode(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ConditionalValue<MailData> mailData;

                using (var trx = this.StateManager.CreateTransaction())
                {
                    mailData = await this.mailQueue.TryPeekAsync(trx);
                }

                if (!mailData.HasValue)
                {
                    await Task.Delay(this.DelayBetweenMailSend , cancellationToken);
                    continue;
                }
                // invio mail mailData;

                using (var trx = this.StateManager.CreateTransaction())
                {
                    mailData = await mailQueue.TryDequeueAsync(trx);
                    await trx.CommitAsync();
                }
            }
        }
        #endregion [ Tasks ] 

        #region [ IMailService interface ]

        public Task<MailServiceError> SendMailAsync(MailInfo mail, CallBackInfo callback, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion [ IMailService interface ]

    }
}
