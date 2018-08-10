using System;
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
using iRentCar.Core;
using iRentCar.InvoiceActor.Interfaces;
using iRentCar.MailService.Interfaces;
using iRentCar.UsersService.Interfaces;
//using iRentCar.UsersService.Interfaces;

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
    internal class UserActor : Core.Implementations.ActorBase, IUserActor, IRemindable, IInvoiceCallbackActor
    {
        public UserActor(ActorService actorService, ActorId actorId)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), null, null)
        {

        }

        public UserActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory, IUsersServiceProxy usersServiceProxy, IInvoicesServiceProxy invoicesServiceProxy)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
            if (usersServiceProxy == null)
                this.usersServiceProxy = UsersServiceProxy.Instance;
            else
                this.usersServiceProxy = usersServiceProxy;

            if (invoicesServiceProxy == null)
                this.invoicesServiceProxy = InvoicesServiceProxy.Instance;
            else
                this.invoicesServiceProxy = invoicesServiceProxy;
        }

        private readonly IUsersServiceProxy usersServiceProxy;
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
                var user = await this.usersServiceProxy.GetUserByUserNameAsync(this.Id.ToString(), default(CancellationToken));
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

        internal const string CurrentRentedCarKeyName = "CurrentRentedCar";
        internal const string InvoikeKeyNamePrefix = "Invoice_";
        internal const string CurrentInvoiceKeyName = "CurrentInvoice";
        internal const string UserDataKeyName = "UserData";
        internal const string MailSendedKeyName = "MailSended";

        #region [ StateManager accessor ]
        private async Task<bool> GetMailSendedFlagFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<bool>(MailSendedKeyName, cancellationToken);
            return infoData.HasValue && infoData.Value;
        }
        private Task SetMailSendedIntoStateAsync(bool mailSended, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<bool>(MailSendedKeyName, mailSended, cancellationToken);
        }

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
            var data = await this.StateManager.TryGetStateAsync<InvoiceData>(CurrentInvoiceKeyName, cancellationToken);
            return data.HasValue ? data.Value : null;
        }
        private Task SetInvoiceDataIntoStateAsync(InvoiceData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<InvoiceData>(CurrentInvoiceKeyName, data, cancellationToken);
        }

        private string GenerateInvoiceKey(string invoiceNumber)
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

            var invoiceKey = GenerateInvoiceKey(data.Number);
            await this.StateManager.SetStateAsync<InvoiceData>(invoiceKey, data, cancellationToken);
        }

        #endregion [ StateManager accessor ]

        #region [ Internal methods ]
        private async Task<bool> IsValidInternalAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var userData = await GetUserDataFromStateAsync(cancellationToken);
            if (userData == null)
                ReportHealthForUserUnknown();
            return userData != null;
        }

        private async Task SendReminderMail(CancellationToken cancellationToken = default(CancellationToken))
        {
            var userInfo = await this.GetUserDataFromStateAsync(cancellationToken);
            if (userInfo != null && !string.IsNullOrWhiteSpace(userInfo.Email))
            {
                var mail = new MailInfo()
                {
                    Subject = $"Your rent reminder!",
                    Body = $"Hi {userInfo.FirstName},<br/>you rent was expired!"
                };
                mail.TOAddresses.Add(userInfo.Email);

                try
                {
                    await MailServiceProxy.Instance.SendMailAsync(mail, null, cancellationToken);
                }
                catch { }
            }
        }
        #endregion [ Internal Methods ]

        #region [ Diagnostics ]
        private void ReportHealthForUserUnknown()
        {
            this.ReportHealthInformation("Identity", "The user is unknown!", HealthState.Warning, 60);
        }
        #endregion [ Diagnostics ]

        #region [ IUserActor interface ]
        public async Task<UserActorError> RentVehicleAsync(RentInfo rentInfo, CancellationToken cancellationToken)
        {
            if (rentInfo == null)
                throw new ArgumentNullException(nameof(rentInfo));

            if (!await IsValidInternalAsync(cancellationToken))
                return UserActorError.UserNotValid;

            UserActorError result = UserActorError.Ok;

            var currentInvoice = await this.GetInvoiceDataFromStateAsync(cancellationToken);
            if (currentInvoice == null)
            {
                var currentRentVehicle = await GetRentDataFromStateAsync(cancellationToken);
                if (currentRentVehicle == null)
                {
                    currentRentVehicle = rentInfo.ToRentData();
                    await SetRentDataIntoStateAsync(currentRentVehicle, cancellationToken);
                    await SetMailSendedIntoStateAsync(false, cancellationToken);
                    var minutesForReminder = rentInfo.EndRent - DateTime.Now;
                    await this.RegisterReminderAsync(EndTimeExpiredReminderName, null, minutesForReminder, TimeSpan.FromMinutes(1));
                }
                else
                {
                    result = UserActorError.VehicleAlreadyRented;
                }
            }
            else
            {
                result = UserActorError.InvoiceNotPaid;
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
                var amount = currentRentVehicle.CalculateCost(DateTime.Now);

                var invoice = await this.invoicesServiceProxy.GenerateInvoiceAsync(this.Id.ToString(), amount.Value,
                   DateTime.Now, UriConstants.UserActorUri, cancellationToken);

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

        public async Task<UserActorError> UpdateUserInfoAsync(UserActorInterfaces.UserInfo info, CancellationToken cancellationToken)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            var userData = new UserData(info);
            await SetUserDataIntoStateAsync(userData, cancellationToken);

            return UserActorError.Ok;
        }

        #endregion [ IUserActor interface ]

        #region [ IInvoiceCallbackActor interface ]
        public async Task InvoicePaidAsync(string invoiceNumber, DateTime paymentDate, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
            {
                var currentInvoice = await this.GetInvoiceDataFromStateAsync(cancellationToken);
                if (currentInvoice.Number == invoiceNumber)
                {
                    currentInvoice.State = InvoiceState.Paid;
                    await this.AddOrUpdateInvoiceIntoStateAsync(currentInvoice, cancellationToken);
                    await this.SetInvoiceDataIntoStateAsync(null, cancellationToken);
                }
            }
        }
        #endregion [ IInvoiceCallbackActor interface ]

        #region [ Reminder ]  

        private const string EndTimeExpiredReminderName = "EndTimeExpiredReminderName";

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName == EndTimeExpiredReminderName)
            {
                var mailSended = await this.GetMailSendedFlagFromStateAsync();
                var rentData = await this.GetRentDataFromStateAsync();

                if (!mailSended && rentData != null)
                {
                    if (rentData.IsRentTimeExpired(DateTime.Now))
                    {
                        var userInfo = await this.GetUserDataFromStateAsync();
                        if (!string.IsNullOrWhiteSpace(userInfo.Email))
                        {
                            await SendReminderMail();
                        }
                        await this.SetMailSendedIntoStateAsync(true);
                    }
                }
                else
                {
                    var reminder = this.GetReminder(reminderName);
                    await this.UnregisterReminderAsync(reminder);
                }
            }
        }

        #endregion [ Reminder ]
    }
}
