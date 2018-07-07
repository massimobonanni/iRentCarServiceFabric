using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.MailService
{
    internal abstract class MailAdapterBase
    {
        public void SetParent(StatefulService parent)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (this.parent != null)
                this.parent.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent -=
                        ConfigurationModified;

            this.parent = parent;

            this.parent.Context.CodePackageActivationContext.ConfigurationPackageModifiedEvent +=
                ConfigurationModified;

            var configPackage = this.parent.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            ConfigurationModified(this,
                new PackageModifiedEventArgs<ConfigurationPackage>() { NewPackage = configPackage, OldPackage = null });
        }

        protected virtual void ConfigurationModified(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {

        }

        protected StatefulService parent;


        public abstract Task<MailAdapterResult> SendMailAsync(MailData mail, CancellationToken cancellationToken);
    }
}
