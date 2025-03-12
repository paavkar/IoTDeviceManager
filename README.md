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

class Sensor {
    +String Id
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String MeasurementType
    +String Unit
    +String? Name

    +String DeviceSerialNumber
}
```

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