# TestConsole commands #

TestConsole allows you to execute few operations in the iRentCar platform.

## Sintax ##
The base sintax for the TesConsole is:

    TestConsole.exe -h
    TestConsole.exe <command> <options>

The first signature allows you to have the TestConsole Help, a complete list of the commands supported by the console.
The second signature allows you to run the specific command. In particular, you can have the help of the single command with:


    TestConsole.exe <command> -h

The next figure shows you the result of the previous command:

![](Documentation/Images/commandhelp.jpg)

## Command: SearchVehicles ##
The command allows you to execute a search to find vehicles in the platform and returns a paged list of the vehicles.


    TestConsole.exe searchVehicles [-plate=<plate>] [-brand=<brand>] [-model=<model>] [-pageSize=<size>] [-pageNumber=<pageNumber>]

Arguments:

- plate : the vehicle plate. The command returns the vehicle with the plate. The argument is not mandatory;
- brand : the vehicle brand. The command returns all the vehicles that contains the filter in the brand field. The argument is not mandatory;
- model : the vehicle model. The command returns all the vehicles that contains the filter in the model field. The argument is not mandatory;
-  pageSize : the number of vehicles returned by the command. The default value is 20;
-  pageNumber : the page number to return. The default value is 1.

Example:

    TestConsole.exe searchVehicles -brand=Audi -pageSize=5 -pageNumber=2

![](documentation/images/SearchVehiclesOutput.jpg)
