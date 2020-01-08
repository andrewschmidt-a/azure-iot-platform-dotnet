# Prerequisites
* .NET Core SDK version 2.2
* Azure CLI

Ensure the `dotnet` and `az` binaries are available in a terminal

# One-Time Setup
Set the `AppConfigurationConnectionString` user secret by running the following in a terminal:
```
dotnet user-secrets set AppConfigurationConnectionString (az appconfig credential list --name app-config-odin -g rg-crslbbiot-odin-dev --query "[?name=='Primary'].connectionString | [0]")
```
And then enumerate secrets:
```
dotnet user-secrets list --project ./common/Services/Services.csproj
AppConfigurationConnectionString = ...
```
# Building
## Build all services
```
dotnet build remote-monitoring.sln
```
## Build an individual service
```
dotnet build ./<service-name>/<service-name>.sln
```
E.g., to build the Storage Adapter service:
```
dotnet build ./storage-adapter/storage-adapter.sln
```

# Running
## Run all services
Use Azure DevSpaces (TODO: document this)
## Run an individual service
The simplest is to use `dotnet run` to spin up a service on a random port on localhost:
```
dotnet run --project ./<service-name>/WebService/WebService.csproj
```
# Debugging
Use either Visual Studio or Visual Studio Code