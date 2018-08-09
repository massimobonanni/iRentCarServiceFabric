# UserActor

It is a Reliable Actor who has the responsibility to manage the status of a user registered in the platform. 

It stores all the user data, the current rent data and all the invoices that the user had.

The user information are:
* Username
* First name
* Last Name
* Emails
* Enabled (true, false)

The user actor also stores:
* the current rent information (vehicle plate, rent start date, rent end date and daily cost);
* the current invoices waiting payment;
* the previous invoices.

The ActorId is the username.

## Requirements
When it is activated, it uses UsesService to retrieve and check its information.

It exposes a feature to check if it is valid or not.

It exposes a feature to retrieve the user information (include the invoice list).

It exposes a feature to reserve a vehicle (see [reserve a vehicle](Scenario-ReserveVehicle.md)) and a feature to release a vehicle (see [release a vehicle](Scenario-ReleaseVehicle.md)).

It exposes a feature to retrieve (if exists) the current invoice.
