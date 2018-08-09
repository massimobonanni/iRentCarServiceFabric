# PaymentService
This component consists of two microservices: PaymentProxy and PaymentGateway.

<br/>

## PaymentProxy
It is a Reliable Stateful Service used by the platform to send a payment request to the external payment system.

### Requirements
It exposes a feature to enqueue a payment request. It must verify the consistency of the request, if the request is valid enqueues it and invokes the external payment system.

### Partitioning
The partition key for each payment request is the hash of the invoice number.

### PaymentProxyProxy
It is the class that exposes the PaymentProxy features and incapsulate the partitioning algorithm. 

Client must use this class instead of creates the service proxy directly (using <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.services.remoting.client.serviceproxy.create?view=azure-dotnet#Microsoft_ServiceFabric_Services_Remoting_Client_ServiceProxy_Create__1_System_Uri_Microsoft_ServiceFabric_Services_Client_ServicePartitionKey_Microsoft_ServiceFabric_Services_Communication_Client_TargetReplicaSelector_System_String_" target="_blank">`ServiceProxy.Create`</a> method).

<br/>

## PaymentGateway
It is a Reliable Stateless Service that hosts the web API application that exposes the WebHook for the payment confirmation.

### Requirements
It exposes a webhook used by the external payment system to comunicate the payment of an invoice (see [invoice payment](Scenario-InvoicePayment.md)).

The webhook must check if the invoice exists and then calls the invoice actor to communicate the payment.