# IoT Device Manager

## Application Overview

This app is meant as an application where you can manage your IoT devices/products.
Current vision includes creation and registering of devices, and then displaying some
basic info about the device and its sensors.

## Running the project locally

Docker is the most obvious requirement for this project. Docker is used with Docker Compose to
orchestrate the different containers. The containers include .NET Web API, MSSQL Server, React
frontend (with Nginx), Angular frontend (with Nginx) and a separate Nginx container for proxy.
Once you have cloned the project, the .NET Web API and Docker require some environment variables.
The application is developed with VS (and VS Code) on Windows.

### Cloning the project

You can clone the project to your desired path with the command
```
git clone https://github.com/paavkar/IoTDeviceManager.git
```

### Environment variables

Docker needs a .env file in the project root with at least the following key-value pairs:
```
DB_CONNECTION_DEV=<INSERT_YOUR_DEV_DB_CONNECTION_STRING>
DB_CONNECTION=<INSERT_YOUR_DB_CONNECTION_STRING>
DatabasePassword=<INSERT_YOUR_DB_PASSWORD>
```
DB_CONNECTION is used in the file docker-compose.prod.yml, if you so choose (NOTE! There
are no actual production containers or container registries for this project currently.)

The .NET Web API requires the following environment variables:
```
"Kestrel:Certificates:Development:Password":<YOUR_PASSOWRD>,
"Jwt:Key":<YOUR_SECRET_KEY>,
"Jwt:Issuer":<YOUR_JWT_ISSUER>,
"Jwt:ExpireMinutes":<JWT_ACCESS_TOKEN_EXPIRY_TIME>,
"Jwt:ExpireDays":<REFRESH_TOKEN_EXPIRY_TIME>,
"Jwt:Audience":<YOUR_JWT_AUDIENCE>,
"ConnectionStrings:DefaultConnection":<YOUR_DB_CONNECTION_STRING>
```
The above formatting is if you use User Secrets (secrets.json) in .NET projects.
The JWT secret key (Jwt:Key) needs to be at least 256 bits long.

## Diagrams

### Class Diagrams

These classes are saved on the SQL Server database, with the attributes that are displayed.

```mermaid
classDiagram
IdentityUser <|-- ApplicationUser

note for IdentityUser "IdentityUser is a class from ASP.NET Core Identity"

class IdentityUser {
    +String Id
    +int AccessFailedCount
    +String? ConcurrencyStamp
    +String? Email
    +bool EmailConfirmed
    +bool LockoutEnabled
    +DateTimeOffset? LockoutEnd
    +String? NormalizedEmail
    +String? NormalizedUserName
    +String? PasswordHash
    +String? PhoneNumber
    +bool PhoneNumberConfirmed
    +String? SecurityStamp
    +bool TwoFactorEnabled
    +String? UserName

    +ToString() String
}

class ApplicationUser

class RefreshToken {
    +String Id
    +String Token
    +DateTimeOffset Expires
    +DateTimeOffset CreatedAt
    +bool Revoked
    +String UserId
}

class Device {
    +String Id
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String Name
    +String SerialNumber
    +String UserId

    +List~Sensor~ Sensors
}

note for Device "Sensors are not saved on the database. They are used only for returning data."

class Sensor {
    +String Id
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String MeasurementType
    +String Unit
    +String? Name
    +double LatestReading

    +String DeviceSerialNumber
}
```

A better approach for data storage would be to at least save the device information on a document-based
database such as Azure Cosmos DB for NoSQL. As Identity doesn't inherently support it, user accounts could
still be stored on SQL Server database. When using this combination, the following structure could be used.

```mermaid
classDiagram

class Device {
    +String Id
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String Name
    +String SerialNumber
    +String UserId

    +List~Sensor~ Sensors
}

class Sensor {
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String? Name
    +List~SensorReading~ LatestReadings

    +String DeviceSerialNumber
}

class SensorReading {
    +String MeasurementType
    +String Unit
    +double Reading
}
```

This would create the following JSON-document on the database:

```
"id": <STRING_VALUE>,
"isOnline": <FALSE_OR_TRUE>,
"lastConnectionTime": <DateTimeOffset_OF_LATEST_CONNECTION>,
"name": <GIVEN_NAME_FOR_THE_DEVICE_BY_USER>,
"serialNumber": <GENERATED_SERIALNUMBER>,
"userId": <ID_OF_THE_USER_WHO_CREATED>,
"type": <STRING_VALUE>,
"partitionKey": <STRING_VALUE>,
"sensors": [
    {
        "isOnline": <FALSE_OR_TRUE>,
        "lastConnectionTime": <DateTimeOffset_OF_LATEST_SENSOR_CONNECTION>,
        "name": <NAME_OF_THE_SENSOR_FROM_DEVICE_SOFTWARE>,
        "latestReadings": [
            {
                "measurementType": <TYPE_OF_MEASUREMENT>,
                "unit": <UNIT_OF_MEASUREMENT>,
                "reading": <READING_FROM_SENSOR>
            }
        ]
    },...
]
```

This kind of setup would allow easier packaging of devices and its sensors. Multiple sensors
are possible with SQL Server, but the benefit of this is to have one sensor for all of the types
of data it measures. For example, an SHT-sensor measures both temperature and humidity, in this
setup, you can save the different form of measurements in the latestReadings array for that
particular sensor.

The before explained format is now deployed with the v2 API. With this structure, the sensor
readings are sent to the database via Azure IoT Hub. When the IoT Hub receives telemetry (sensor)
data, an Azure Function is used to save the data on the database. The Azure Function code is available
in this repository, and the example of how to communicate with Azure IoT Hub is available at
[EmbeddedProjects | AzureIoTHub](https://github.com/paavkar/EmbeddedProjects/tree/main/AzureIoTHub).
The code here is largely the same as Microsoft's example code.

### State Diagrams

This diagram displays the flow of application usage from being logged out and not having an account
to being logged in.

```mermaid
stateDiagram-v2
    state loggedInState <<choice>>
    state loginSuccessfulState <<choice>>
    state registeredState <<choice>>
    state registerSuccessfulState <<choice>>
    state creationSuccessfulState <<choice>>
    state deviceRegisterSuccessfulState <<choice>>
    state deviceExistsState <<choice>>

    [*] --> /
    / --> loggedInState : is user logged in
    loggedInState --> /login : is not logged in
    /login --> loginSuccessfulState : is login successful
    loginSuccessfulState --> LoggedIn : login is successful
    loginSuccessfulState --> /login : login is unsuccessful (eror is displayed)
    loggedInState --> LoggedIn : is logged in

    /login --> registeredState : is user registered
    registeredState --> /register : user is not registered
    /register --> registerSuccessfulState : is register successful
    registerSuccessfulState --> /login : user registered account successfully
    registerSuccessfulState --> /register : register is unsuccessful (error is displayed)

    state LoggedIn {
        AddDevice: AddDevice (dialog)
        RegisterExistingDevice: RegisterExistingDevice (dialog)
        CreateNewDevice: CreateNewDevice (dialog)

        [*] --> /home
        /home --> /devices
        /devices --> AddDevice
        AddDevice --> deviceExistsState : does device (serial number) already exist
        deviceExistsState --> CreateNewDevice : does not exist
        CreateNewDevice --> creationSuccessfulState : was device (serial number created)
        creationSuccessfulState --> /devices : creation is successful
        creationSuccessfulState --> CreateNewDevice : there was an error (display it)
        
        deviceExistsState --> RegisterExistingDevice : exists
        RegisterExistingDevice --> deviceRegisterSuccessfulState : is device registration successful
        deviceRegisterSuccessfulState --> /devices : registration is successful
        deviceRegisterSuccessfulState --> RegisterExistingDevice : there was an error (display it)

        /devices --> /[id]
    }
```

## App Usage

To use the application, you first need to register, and then login. After logging in, you can create
a device with a name. Creating a device saves a device with your given name, and a serial number is
generated for it. Your devices are displayed in the devices page.

To send data from your IoT device, you need the serial number of your created device. Sending data is based on the
different sensors and what they measure. As an example, if your device has an SHT-sensor that measures
both temperature and humidity, these measurement types would count as different sensors.

You can see an example of how to send data using an Arduino-based board with different sensors here:
[GitHub | EmbeddedProjects](https://github.com/paavkar/EmbeddedProjects/blob/main/IoTServices/src/main.cpp).

To test the API locally, you send the request to ``<APPLICATION_HOST_IP>/api/v1/Device/<DEVICE_SERIAL_NUMBER>``.
Depending on your embedded setup, your configuration could differ. If you use similar setup with ArduinoHttpClient
as I have used (see link), you should have minimal issues. NOTE on ArduinoHttpClient: you don't need to manually add
http:// before the IP.

The API expects a request body of following key-value pairs:
```
name: <YOUR_NAME_FOR_THE_SENSOR>,
measurementType: <WHAT_IS_MEASURED>,
unit: <UNIT_OF_MEASUREMENT>,
latestReading: <THE_LATEST_READING_FROM_SENSOR>
```
The following is an example of values:
```
name: shtc3_temp,
measurementType: Temperature,
unit: C,
latestReading: 23.1
```
If you want to use the newer Azure Cosmos DB based solution, send the request to the v2 endpoint.