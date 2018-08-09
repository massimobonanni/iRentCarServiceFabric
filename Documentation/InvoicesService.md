# InvoicesService

It is a Reliable Stateful Service that exposes the features to generate invoice numbers and new invoices.

UserActor uses this service to generate a new invoice when he returns a vehicle (see [release a vehicle](Scenario-ReleaseVehicle.md)).

The service doesn't store any information about invoices created.

## Requirements
The service stores only the current number of the invoice per year.

It exposes feature to generate a new invoice:
* generates the next sequence number for the current year;
* creates an invoice number concatenates the year with the sequence (e.g. `"2018/123"`)
* creates an invoice actor using CreateInvoice method
* returns the invoice to the caller


## Partitioning
The service has only one pertition.

## UsersServiceProxy
It is the class that exposes the InvoicessService features and incapsulate the partitioning algorithm. 

Client must use this class instead of creates the service proxy directly (using <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.services.remoting.client.serviceproxy.create?view=azure-dotnet#Microsoft_ServiceFabric_Services_Remoting_Client_ServiceProxy_Create__1_System_Uri_Microsoft_ServiceFabric_Services_Client_ServicePartitionKey_Microsoft_ServiceFabric_Services_Communication_Client_TargetReplicaSelector_System_String_" target="_blank">`ServiceProxy.Create`</a> method).
