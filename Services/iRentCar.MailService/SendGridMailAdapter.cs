using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.MailService.SendGrid;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;
using RestSharp;

namespace iRentCar.MailService
{
    internal class SendGridMailAdapter : MailAdapterBase
    {
        private string sendMailUri;
        private string apiKey;
        private string fromAddress;
        private bool configurationCorrect;

        private const string ConfigurationHealthPropertyName = "Configuration";

        protected override void ConfigurationModified(object sender, PackageModifiedEventArgs<ConfigurationPackage> e)
        {
            if (e.NewPackage.Settings.Sections.Contains("SendGridAdapter"))
            {
                var configSection = e.NewPackage.Settings.Sections["SendGridAdapter"];
                this.configurationCorrect = GetConfigValue<string>(configSection, "SendMailUri", ref sendMailUri);
                this.configurationCorrect &= GetConfigValue<string>(configSection, "ApiKey", ref apiKey);
                this.configurationCorrect &= GetConfigValue<string>(configSection, "FromAddress", ref fromAddress);

                if (string.IsNullOrWhiteSpace(this.sendMailUri) && Uri.IsWellFormedUriString(this.sendMailUri, UriKind.Absolute))
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                            $"Parameter 'SendMailUri'not valid!", System.Fabric.Health.HealthState.Error);
                    this.configurationCorrect = false;
                }

                if (string.IsNullOrWhiteSpace(this.apiKey))
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                            $"Parameter 'ApiKey'not valid!", System.Fabric.Health.HealthState.Error);
                    this.configurationCorrect = false;
                }

                if (string.IsNullOrWhiteSpace(this.fromAddress))
                {
                    this.parent.ReportHealthInformation(ConfigurationHealthPropertyName,
                            $"Parameter 'FromAddress'not valid!", System.Fabric.Health.HealthState.Error);
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
                    "Configuration section 'SendGridAdapter' not exists!", System.Fabric.Health.HealthState.Error);
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
                MailMessage mailMessage = CreateMailMessage(mail);

                var client = new RestClient(this.sendMailUri);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/json");
                request.AddHeader("authorization", $"Bearer {this.apiKey}");
                request.AddParameter("application/json",
                    JsonConvert.SerializeObject(mailMessage), ParameterType.RequestBody);
                var response = await client.ExecuteTaskAsync(request);
                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                    return MailAdapterResult.NotInfrastructuralError;
            }
            catch (Exception ex)
            {
                return MailAdapterResult.NotInfrastructuralError;
            }
            return MailAdapterResult.Ok;
        }

        private MailMessage CreateMailMessage(MailData mailData)
        {
            var mailMessage = new MailMessage();
            var personalization = new Personalization();
            personalization.subject = mailData.Subject;
            if (mailData.TOAddresses != null && mailData.TOAddresses.Any())
            {
                personalization.to = new List<To>();
                foreach (var address in mailData.TOAddresses)
                {
                    personalization.to.Add(new To() { email = address });
                }
            }

            if (mailData.BccAddresses != null && mailData.BccAddresses.Any())
            {
                personalization.bcc = new List<Bcc>();
                foreach (var address in mailData.BccAddresses)
                {
                    personalization.bcc.Add(new Bcc() { email = address });
                }
            }
            if (mailData.CCAddresses != null && mailData.CCAddresses.Any())
            {
                personalization.cc = new List<Cc>();
                foreach (var address in mailData.CCAddresses)
                {
                    personalization.cc.Add(new Cc() { email = address });
                }
            }
            mailMessage.personalizations = new List<Personalization>();
            mailMessage.personalizations.Add(personalization);
            mailMessage.from = new From() { email = mailData.From ?? this.fromAddress };
            mailMessage.content = new List<Content>();
            mailMessage.content.Add(new Content()
            {
                type = mailData.IsHtml ? "text/html" : "text/plain",
                value = mailData.Body
            });
            return mailMessage;
        }
    }
}
