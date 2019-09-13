<#
    .DESCRIPTION
        A run book for creating a new IoT Hub for a tenant

    .NOTES
        AUTHOR: Nate Oelke
                Sean Dubiel
#>

param
(
    [Parameter (Mandatory = $false)]
    [object] $WebhookData
)

# Make sure this runbook was triggered by a webhook
if (-Not $WebhookData) {
    # Error
    Write-Error "This runbook is meant to be started from an Azure alert webhook only."
}

# Authenticate with the service principle
$connectionName = "AzureRunAsConnection"
try
{
    $servicePrincipalConnection = Get-AutomationConnection -Name $connectionName

    "Logging in to Azure..."
    $connectionResult =  Connect-AzAccount -Tenant $servicePrincipalConnection.TenantID `
                             -ApplicationId $servicePrincipalConnection.ApplicationID   `
                             -CertificateThumbprint $servicePrincipalConnection.CertificateThumbprint `
                             -ServicePrincipal
    "Logged in."
}
catch {
    if (!$servicePrincipalConnection)
    {
        $ErrorMessage = "Connection $connectionName not found."
        throw $ErrorMessage
    } else{
        Write-Error -Message $_.Exception
        throw $_.Exception
    }
}

# Retrieve the data from the Webhook request body
$data = (ConvertFrom-Json -InputObject $WebhookData.RequestBody)
$appConfigConnectionString = $data.appConfigConnectionString
$setAppConfigEndpoint = $data.setAppConfigEndpoint
$data.token

$requestheader = @{
  "Authorization" = "Bearer " + $data.token
  "Content-Type" = "application/json"
}

$iotHubUri = "https://management.azure.com/subscriptions/$($data.subscriptionId)/resourceGroups/$($data.resourceGroup)/providers/Microsoft.Devices/IotHubs/$($data.iotHubName)?api-version=2019-03-22-preview"
# Delete IoT Hub using Azure REST API
$result = (Invoke-RestMethod -Method delete -Headers $requestheader -Uri $iotHubUri)

# Delete the IoT Hub connection string to app config
$requestheader = @{
  "Content-Type" = "application/json"
}
$appConfigKey= "tenant:$($data.tenantId):iotHubConnectionString"

$appConfigBody = @"
{
     connectionstring : "$appConfigConnectionString", name : "$appConfigKey",
}
"@
$appConfigBody
$result = (Invoke-RestMethod -ContentType 'application/json' -Method delete -Headers $requestheader -Uri $setAppConfigEndpoint -Body $appConfigBody)

$dbname = $data.databaseName
$collectionNameTelemetry = $data.telemetryCollectionName
$collectionNameTwin = $data.twinChangeCollectionName
$collectionNameLife = $data.lifecycleCollectionName

$requestheader = @{
  "Authorization" = "Bearer " + $data.token
  "Content-Type" = "application/json"
}
$CosmosDb = $data.CosmosDb
$collectionName = $data.telemetryCollectionName
$uri = $CosmosDb + $dbname + "/colls" + "/" + $collectionName
$collectionName = $dbname + "/colls" + "/" + $collectionName
#$hdrs = BuildHeaders -action DELETE -resType colls -resourceId $collectionName
#Write-Host "resourceId $collectionName"
$CosmosDb
$uri
$response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $requestheader

$collectionName = $data.twinChangeCollectionName
$uri = $CosmosDb + $dbname + "/colls" + "/" + $collectionName
$collectionName = $dbname + "/colls" + "/" + $collectionName
$uri
$response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $requestheader

$collectionName = $data.lifecycleCollectionName
$uri = $CosmosDb + $dbname + "/colls" + "/" + $collectionName
$collectionName = $dbname + "/colls" + "/" + $collectionName
$uri
$response = Invoke-RestMethod -Uri $uri -Method Delete -Headers $requestheader

"Done"