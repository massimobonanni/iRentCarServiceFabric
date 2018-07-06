using iRentCar.MailService.Interfaces;
using System;
using System.Runtime.Serialization;

namespace iRentCar.MailService
{
    [DataContract]
    internal class CallBackData
    {
        public CallBackData()
        {

        }

        public CallBackData(CallBackInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.ActorId = info.ActorId;
            this.ServiceUri = info.ServiceUri;
        }

        [DataMember]
        public string ActorId { get; set; }
        [DataMember]
        public string ServiceUri { get; set; }
    }
}