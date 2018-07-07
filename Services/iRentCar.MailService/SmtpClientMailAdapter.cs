using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace iRentCar.MailService
{
    internal class SmtpClientMailAdapter : MailAdapterBase
    {
        private string mailServer;
        private int mailPort;
        private string username;
        private string password;
        private bool useSSL;
        private bool configurationCorrect;

        private const string ConfigurationHealthPropertyName = "Configuration";

        protected override void ConfigurationModified(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            if (e.NewPackage.Settings.Sections.Contains("NetSmtpClient"))
            {
                var configSection = e.NewPackage.Settings.Sections["NetSmtpClient"];
                this.configurationCorrect = GetConfigValue<string>(configSection, "MailServer", ref mailServer);
                this.configurationCorrect &= GetConfigValue<int>(configSection, "MailPort", ref mailPort);
                this.configurationCorrect &= GetConfigValue<string>(configSection, "Username", ref username);
                this.configurationCorrect &= GetConfigValue<string>(configSection, "Password", ref password);
                this.configurationCorrect &= GetConfigValue<bool>(configSection, "UseSSL", ref useSSL);

                if (string.IsNullOrWhiteSpace(this.mailServer))
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                            $"Parameter 'MailServer' empty or null!", System.Fabric.Health.HealthState.Error);
                    this.configurationCorrect = false;
                }

                if (mailPort <= 0)
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                            $"Parameter 'MailPort' negative!", System.Fabric.Health.HealthState.Error);
                    this.configurationCorrect = false;
                }

                if (this.configurationCorrect)
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                        $"Configuration correct!", System.Fabric.Health.HealthState.Ok);
                }
            }
            else
            {
                this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                    "Configuration section 'NetSmtpClient' not exists!", System.Fabric.Health.HealthState.Error);
            }
        }

        private bool GetConfigValue<T>(ConfigurationSection section, string paramName, ref T value)
        {
            bool retval = false;
            value = default(T);
            if (section.Parameters.Contains(paramName))
            {
                var configValue = section.Parameters[paramName].Value;
                try
                {
                    value = (T)Convert.ChangeType(configValue, typeof(T));
                    retval = true;
                }
                catch { }
            }
            else
            {
                this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                    $"Parameter '{paramName}' not valid!", System.Fabric.Health.HealthState.Error);
            }

            return retval;
        }


        public override async Task<MailAdapterResult> SendMailAsync(MailData mail, CancellationToken cancellationToken)
        {
            if (!this.configurationCorrect)
            {
                return MailAdapterResult.InfrastructuralError;
            }
            try
            {
                using (var client = new SmtpClient(this.mailServer, this.mailPort))
                {
                    client.Credentials = new NetworkCredential(this.username, this.password);
                    client.EnableSsl = this.useSSL;

                    MailMessage mailMessage = CreateMailMessage(mail);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception)
            {
                return MailAdapterResult.NotInfrastructuralError;
            }
            return MailAdapterResult.Ok;
        }

        private MailMessage CreateMailMessage(MailData mailData)
        {
            var mailMessage = new MailMessage();
            mailMessage.Body = mailData.Body;
            mailMessage.Subject = mailData.Subject;
            mailMessage.From = new MailAddress(mailData.From);
            mailMessage.IsBodyHtml = mailData.IsHtml;
            if (mailData.TOAddresses != null && mailData.TOAddresses.Any())
            {
                foreach (var address in mailData.TOAddresses)
                {
                    mailMessage.To.Add(address);
                }
            }
            if (mailData.BccAddresses != null && mailData.BccAddresses.Any())
            {
                foreach (var address in mailData.BccAddresses)
                {
                    mailMessage.Bcc.Add(address);
                }
            }
            if (mailData.CCAddresses != null && mailData.CCAddresses.Any())
            {
                foreach (var address in mailData.CCAddresses)
                {
                    mailMessage.CC.Add(address);
                }
            }
            return mailMessage;
        }
    }
}
