# VehicleActor

It is a Reliable Actor who has the responsibility to manage the status of a single vehicle. It stores all the vehicle data, its state (Available, NotAvailable and Busy) and the data of the user rents the vehicle.

The vehicle information are:
* Plate
* Model
* Brand
* Status (Free, Busy, Not Available)

The vehicle actor also stores the information about the current rent (username, reservation start date and reservation end date).

The ActorId is the plate.

## Requiremnents
It exposes a feature to reserve the vehicle (see [reserve a vehicle](Scenario-ReserveVehicle.md)). The feature accepts in input the user, the start date and the end date. A vehicle can be reserved if:
* the vehicle is free; 
* the user is valid; 
* the user hasn't another vehicle;
* the user has paid all previous invoices.

It exposes a feature to return the vehicle (see [release a vehicle](Scenario-ReleaseVehicle.md)).

It exposes a feature to retrieve the vehicle information.



