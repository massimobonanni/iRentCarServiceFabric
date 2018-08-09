# Requirements

iRentCar is a simple demo, and the requirements are a simplification of real requirements for a similar platform.

You have to consider the following requirements as a sample to simulate the real ones.

* The platform must have a web UI to manage vehicles and users. The UI is used by the desk users to do the operations.
* The platform must have a search feature used by the desk users to find a specific vehicle and a set of features to reserve and unreserve the vehicle for a registered user.
* A vehicle can be reserved if it is free, the user must be a valid user, he doesn't rent another vehicle, and he must have paid all the previous invoices.
* An invoice is generated when a vehicle is returned and it is associated with the user.
* A user can pay with an external system that calls a WebHook exposed by the platform to confirm the payment process.
* The platform sends an ema
il to the user when the rental period is expired.