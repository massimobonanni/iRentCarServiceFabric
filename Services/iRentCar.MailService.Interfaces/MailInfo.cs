using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace iRentCar.MailService.Interfaces
{
    [DataContract]
    public class MailInfo
    {
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


    }
}
