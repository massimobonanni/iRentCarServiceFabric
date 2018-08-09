# InvoiceActor
It is a Reliable Actor who has the responsibility to manage a single invoice for the platform. 

It stores the invoice's data and its state.


The invoice information are:
* Invoice number
* Amount
* Status (Paid, not Paid)
* Creation date
* Payment date
* User to link with

The ActorId is the invoice number.

## Requirements
It exposes a feature to create the invoice. This feature is used by the InvoicesService to generate a new invoice (see [release a vehicle](Scenario-ReleaseVehicle.md)) . If the create feature is called over an existing invoice (created previously), it returns an error.

It exposes a feature to confirm the payment. This feature is used by the PaymentGateway when the external payment system calls the webhook (see [invoice payment](Scenario-InvoicePayment.md)) 

It exposes a feature to retrieve the invoice information.
