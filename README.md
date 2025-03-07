# IoT Device Manager

## Application Overview

This app is meant as an application where you can manage your IoT devices/products.

This app is set up with Docker containers. Docker Compose is used to orchestrate to starting of the needed containers.
There are three containers in total, one for the database (Microsoft SQL Server), one for the backend (.NET),
and finally one for the frontend (React). The React frontend's Dockerfile is set up to build the project files
and then serve them with Nginx.

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