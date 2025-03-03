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

    +List~Sensor~ Sensors
}

class Sensor {
    +String Id
    +bool IsOnline
    +DateTimeOffset? LastConnectionTime
    +String? MeasurementType
    +String Name

    +String DeviceId
}
```