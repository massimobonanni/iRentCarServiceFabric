# MailSender

It is a Reliable Stateful Service that exposes featureto send mail.

UserActor uses this service to send an email to the user when the rental period is expired.


## Requirements
The service enqueues send email requests and sends each of this in best effort. 

It exposes a feature to enqueue a send email request. This function returns true and enqueue the request only if the request is valid.

## Partitioning
The partitioning is using only for parallelize the operations.

## UsersServiceProxy
It is the class that exposes the SendMail features and incapsulate the partitioning algorithm. 

The proxy uses a *round-robin* algorithm to choose which partition it must use. The first request go to the first partition, the second one to the second partition and so on. In this case, the number of partitions represents the parallelism of the service

Client must use this class instead of creates the service proxy directly (using <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.services.remoting.client.serviceproxy.create?view=azure-dotnet#Microsoft_ServiceFabric_Services_Remoting_Client_ServiceProxy_Create__1_System_Uri_Microsoft_ServiceFabric_Services_Client_ServicePartitionKey_Microsoft_ServiceFabric_Services_Communication_Client_TargetReplicaSelector_System_String_" target="_blank">`ServiceProxy.Create`</a> method).
