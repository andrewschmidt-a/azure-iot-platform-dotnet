# Prerequisites
* .NET Core SDK version 2.2
* Azure CLI

Ensure the `dotnet` and `az` binaries are available in a terminal

# One-Time Setup
Ensure the `AppConfigurationConnectionString` is set before building. This can be done in one of two ways:

1. Set an environment variable
1. Use `dotnet user-secrets`

Either way, you will need to choose an Azure App Configuration instance and make note of its `<name>` and `<resource-group>` for use in the steps below.

## Set an environment variable
### Windows
In a PowerShell shell:
```
[System.Environment]::SetEnvironmentVariable('AppConfigurationConnectionString', (az appconfig credential list --name <name> --resource-group <resource-group> --query "[?name=='Primary'].connectionString | [0]" --output tsv), 'User')
```

### Non-Windows
Set the `AppConfigurationConnectionString` environment variable in the Bash configuration file of your choice.

## Use dotnet user-secrets
After setting the user secret below, you can check that it is set properly as follows:
```
dotnet user-secrets list --project ./common/Services/Services.csproj
```
### Windows
In a PowerShell shell:
```
dotnet user-secrets set --project ./common/Services/Services.csproj AppConfigurationConnectionString (az appconfig credential list --name <name> --resource-group <resource-group> --query "[?name=='Primary'].connectionString | [0]" --output tsv)
```

### Non-Windows
In a Bash shell:
```
dotnet user-secrets set --project ./common/Services/Services.csproj AppConfigurationConnectionString `az appconfig credential list --name <name> --resource-group <resource-group> --query "[?name=='Primary'].connectionString | [0]" --output tsv`
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