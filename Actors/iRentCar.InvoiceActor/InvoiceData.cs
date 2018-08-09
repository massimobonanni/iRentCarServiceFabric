using iRentCar.InvoiceActor.Interfaces;
using System;
using System.Runtime.Serialization;

namespace iRentCar.InvoiceActor
{
    [DataContract]
    internal class InvoiceData
    {
        public InvoiceData()
        {
            
        }

        public InvoiceData(InvoiceData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            this.Amount = data.Amount;
            this.CallbackUri = data.CallbackUri;
            this.CreationDate = data.CreationDate;
            this.CustomerId = data.CustomerId;
            this.PaymentDate = data.PaymentDate;
            this.State = data.State;
        }

        [DataMember]
        public string CallbackUri { get; set; }

        [DataMember]
        public string CustomerId { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime? PaymentDate { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public InvoiceState State { get; set; }


    }
}