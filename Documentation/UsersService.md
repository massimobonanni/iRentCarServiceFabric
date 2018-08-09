# UsersService

It is a Reliable Stateful Service that exposes search features for the registered users of the platform.

UserActor uses this service to check if it is a valid user or not.

The web UI uses this service to search users.

## Requirements
The service stores the users information and use them to expose search feature.

It exposes a search feature that allow the client to retrieve a list of users filtered by username, first name, last name or email.

It exposes a feature to retrieve a user using its username. This feature is used by UserActor to verify if it is valid or not.

It exposes a feature to add or update a user.

## Partitioning
The partition key for each user is the hash code of the username.

## UsersServiceProxy
It is the class that exposes the UsersService features and incapsulate the partitioning algorithm. 

Client must use this class instead of creates the service proxy directly (using <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.services.remoting.client.serviceproxy.create?view=azure-dotnet#Microsoft_ServiceFabric_Services_Remoting_Client_ServiceProxy_Create__1_System_Uri_Microsoft_ServiceFabric_Services_Client_ServicePartitionKey_Microsoft_ServiceFabric_Services_Communication_Client_TargetReplicaSelector_System_String_" target="_blank">`ServiceProxy.Create`</a> method).
