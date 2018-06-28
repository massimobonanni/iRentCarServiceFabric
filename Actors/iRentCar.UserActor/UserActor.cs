﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Implementations;
using iRentCar.Core.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using UserActorInterfaces = iRentCar.UserActor.Interfaces;
using InvoicesServiceInterfaces = iRentCar.InvoicesService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using iRentCar.UserActor.Interfaces;
using System.Fabric.Health;
using System.Fabric;

namespace iRentCar.UserActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    [ActorService(Name = "UserActor")]
    internal class UserActor : Core.Implementations.ActorBase, IUserActor
    {
        public UserActor(ActorService actorService, ActorId actorId)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), new InMemoryUserRepository(), null)
        {

        }

        public UserActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory, IUsersRepository userRepository, IInvoicesServiceProxy invoicesServiceProxy)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
            if (userRepository == null)
                throw new ArgumentNullException(nameof(userRepository));
            this.userRepository = userRepository;

            if (invoicesServiceProxy == null)
                this.invoicesServiceProxy = InvoicesServiceProxy.Instance;
            else
                this.invoicesServiceProxy = invoicesServiceProxy;
        }

        private readonly IUsersRepository userRepository;
        private readonly IInvoicesServiceProxy invoicesServiceProxy;

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            var userData = await GetUserDataFromStateAsync();

            if (userData == null)
            {
                var user = await this.userRepository.GetUserByUsernameAsync(this.Id.ToString(), default(CancellationToken));
                if (user != null)
                {
                    userData = new UserData()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        IsEnabled = user.IsEnabled
                    };
                    await SetUserDataIntoStateAsync(userData);
                }
                else
                {
                    this.ReportHealthForUserUnknown();
                }
            }

        }


        private const string CurrentRentedCarKeyName = "CurrentRentedCar";
        private const string InvoikeKeyNamePrefix = "Invoice_";
        private const string CurrentInvoikeKeyName = "CurrentInvoice";
        private const string UserDataKeyName = "UserData";

        #region [ StateManager accessor ]
        private async Task<UserData> GetUserDataFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<UserData>(UserDataKeyName, cancellationToken);
            return infoData.HasValue ? infoData.Value : null;
        }
        private Task SetUserDataIntoStateAsync(UserData info, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<UserData>(UserDataKeyName, info, cancellationToken);
        }

        private async Task<RentData> GetRentDataFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await this.StateManager.TryGetStateAsync<RentData>(CurrentRentedCarKeyName, cancellationToken);
            return data.HasValue ? data.Value : null;
        }
        private Task SetRentDataIntoStateAsync(RentData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<RentData>(CurrentRentedCarKeyName, data, cancellationToken);
        }

        private async Task<InvoiceData> GetInvoiceDataFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await this.StateManager.TryGetStateAsync<InvoiceData>(CurrentInvoikeKeyName, cancellationToken);
            return data.HasValue ? data.Value : null;
        }
        private Task SetInvoiceDataIntoStateAsync(InvoiceData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<InvoiceData>(CurrentInvoikeKeyName, data, cancellationToken);
        }

        private string GenerateInvoice(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                throw new ArgumentException(nameof(invoiceNumber));

            return $"{InvoikeKeyNamePrefix}{invoiceNumber}";
        }

        private async Task<IEnumerable<string>> GetInvoiceKeysAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return (await this.StateManager.GetStateNamesAsync(cancellationToken)).Where(k => k.StartsWith(InvoikeKeyNamePrefix));
        }

        private async Task AddOrUpdateInvoiceIntoStateAsync(InvoiceData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            var invoiceKey = GenerateInvoice(data.Number);
            await this.StateManager.SetStateAsync<InvoiceData>(invoiceKey, data, cancellationToken);
        }

        #endregion [ StateManager accessor ]

        #region [ Diagnostics ]
        private void ReportHealthForUserUnknown()
        {
            this.ReportHealthInformation("Identity", "The user is unknown!", HealthState.Warning, 60);
        }
        #endregion [ Diagnostics ]

        #region [ IUserActor interface ]
        private async Task<bool> IsValidInternalAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var userData = await GetUserDataFromStateAsync(default(CancellationToken));
            if (userData == null)
                ReportHealthForUserUnknown();
            return userData != null;
        }

        public async Task<UserActorError> RentVehicleAsync(RentInfo rentInfo, CancellationToken cancellationToken)
        {
            if (rentInfo == null)
                throw new ArgumentNullException(nameof(rentInfo));

            if (!await IsValidInternalAsync(cancellationToken))
                return UserActorError.UserNotValid;

            UserActorError result = UserActorError.Ok;

            var currentRentVehicle = await GetRentDataFromStateAsync(cancellationToken);

            if (currentRentVehicle == null)
            {
                currentRentVehicle = new RentData()
                {
                    StartRent = rentInfo.StartRent,
                    VehicleDailyCost = rentInfo.DailyCost,
                    VehiclePlate = rentInfo.Plate
                };
                await SetRentDataIntoStateAsync(currentRentVehicle, cancellationToken);
            }
            else
            {
                result = UserActorError.VehicleAlreadyRented;
            }

            return result;

        }

        public async Task<UserActorError> ReleaseVehicleAsync(CancellationToken cancellationToken)
        {
            if (!await IsValidInternalAsync(cancellationToken))
                return UserActorError.UserNotValid;

            UserActorError result = UserActorError.Ok;

            var currentRentVehicle = await GetRentDataFromStateAsync(cancellationToken);

            if (currentRentVehicle != null)
            {
                currentRentVehicle.EndRent = DateTime.Now;
                var amount = currentRentVehicle.CalculateCost();

                var invoice = await this.invoicesServiceProxy.GenerateInvoiceAsync(this.Id.ToString(), amount.Value,
                    currentRentVehicle.EndRent.Value, cancellationToken);

                var localInvoice = new InvoiceData()
                {
                    Amount = invoice.Amount,
                    Number = invoice.InvoiceNumber,
                    ReleaseDate = invoice.ReleaseDate,
                    State = invoice.State == InvoicesServiceInterfaces.InvoiceState.Paid ? InvoiceState.Paid : InvoiceState.NotPaid,
                };

                await SetInvoiceDataIntoStateAsync(localInvoice, cancellationToken);
                await SetRentDataIntoStateAsync(null, cancellationToken);
            }
            else
            {
                result = UserActorError.VehicleNotRented;
            }

            return result;
        }

        public Task<bool> IsValidAsync(CancellationToken cancellationToken)
        {
            return this.IsValidInternalAsync(cancellationToken);
        }

        public async Task<UserActorInterfaces.InvoiceInfo> GetActiveInvoiceAsync(CancellationToken cancellationToken)
        {
            var invoice = await GetActiveInvoiceAsync(cancellationToken);
            return invoice;
        }

        public async Task<Interfaces.UserInfo> GetInfoAsync(CancellationToken cancellationToken)
        {
            if (!await IsValidInternalAsync(cancellationToken))
                return null;

            var userInfo = (await this.GetUserDataFromStateAsync(cancellationToken)).ToUserInfo();

            var rentData = await this.GetRentDataFromStateAsync(cancellationToken);
            if (rentData != null)
            {
                userInfo.CurrentRent = rentData.ToRentInfo();
            }

            var currentInvoice = await this.GetInvoiceDataFromStateAsync(cancellationToken);
            if (currentInvoice != null)
            {
                userInfo.Invoices.Add(currentInvoice.ToInvoiceInfo());
            }

            var invoiceKeys = await this.GetInvoiceKeysAsync(cancellationToken);
            if (invoiceKeys != null && invoiceKeys.Any())
            {
                foreach (var key in invoiceKeys)
                {
                    var invoice = await this.StateManager.GetStateAsync<InvoiceData>(key, cancellationToken);
                    userInfo.Invoices.Add(invoice.ToInvoiceInfo());
                }
            }

            return userInfo;
        }
        #endregion [ IUserActor interface ]

        protected void ReportHealthInformation(string sourceId, string property, string description,
            HealthState state, int secondsToLive)
        {
            HealthInformation healthInformation = new HealthInformation(sourceId, property, state);
            healthInformation.Description = description;
            if (secondsToLive > 0) healthInformation.TimeToLive = TimeSpan.FromSeconds(secondsToLive);
            healthInformation.RemoveWhenExpired = true;
            try
            {
                this.ActorService.Context.CodePackageActivationContext.re
                var activationContext = FabricRuntime.GetActivationContext();
                activationContext.ReportApplicationHealth(healthInformation);
            }
            catch { }
        }
    }
}
