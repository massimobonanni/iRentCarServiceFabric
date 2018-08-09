# VehiclesService

It is a Reliable Stateful Service that exposes search features for the vehicles of the platform.

VehicleActor uses this service to check if it is a valid vehicle or not and interacts with it to update its status.

The web UI uses this service to search vehicles.

## Requirements
The service stores the vehicles information and use them to expose search feature.

It exposes a search feature that allow the client to retrieve a list of vehicles filtered by plate, state, brand and/or model.

It exposes a feature to update a vehicle state used by the VehicleActor (see [reserve a vehicle](Scenario-ReserveVehicle.md) or [release a vehicle](Scenario-ReleaseVehicle.md)) when a vehicle is reserved or returned.

It exposes a feature to retrieve a vehicle using its plate. This feature is used by VehicleActor to verify if it exists or not.

It exposes a feature to add or update a vehicle.

## Partitioning
The partition key for each vehicle is the hash code of the plate.

## VehiclesServiceProxy
It is the class that exposes the VehiclesService features and incapsulate the partitioning algorithm. 

Client must use this class instead of creates the service proxy directly (using <a href="https://docs.microsoft.com/en-us/dotnet/api/microsoft.servicefabric.services.remoting.client.serviceproxy.create?view=azure-dotnet#Microsoft_ServiceFabric_Services_Remoting_Client_ServiceProxy_Create__1_System_Uri_Microsoft_ServiceFabric_Services_Client_ServicePartitionKey_Microsoft_ServiceFabric_Services_Communication_Client_TargetReplicaSelector_System_String_" target="_blank">`ServiceProxy.Create`</a> method).