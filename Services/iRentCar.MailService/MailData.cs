using iRentCar.MailService.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace iRentCar.MailService
{
    [DataContract]
    internal class MailData
    {
        public MailData()
        {

        }
        public MailData(MailInfo info, CallBackInfo callBack)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.Id = info.Id;
            this.From = info.From;
            this.BccAddresses = info.BccAddresses;
            this.Body = info.Body;
            this.CCAddresses = info.CCAddresses;
            this.IsHtml = info.IsHtml;
            this.Subject = info.Subject;
            this.TOAddresses = info.TOAddresses;
            if (callBack != null)
                this.CallBack = new CallBackData(callBack);
        }

        [DataMember]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DataMember]
        public string From { get; set; }
        [DataMember]
        public List<string> TOAddresses { get; set; }
        [DataMember]
        public List<string> CCAddresses { get; set; }
        [DataMember]
        public List<string> BccAddresses { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Body { get; set; }

        [DataMember]
        public bool IsHtml { get; set; }

        [DataMember]
        public CallBackData CallBack { get; set; }


    }
}
