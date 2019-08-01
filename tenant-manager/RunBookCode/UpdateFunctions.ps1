<#
    .DESCRIPTION
        A run book for adding a new Cosmos DB output to the 3 Azure Functions (Telemetry, Lifecycle, and Twin Change)

    .NOTES
        AUTHOR: Nate Oelke
#>

param
(
    [Parameter (Mandatory = $false)]
    [object] $WebhookData
)

# If runbook was called from Webhook, WebhookData will not be null.
if (-Not $WebhookData) {
    # Error
    write-Error "This runbook is meant to be started from a webhook only."
}

# Retrieve the data from the Webhook request body
$data = (ConvertFrom-Json -InputObject $WebhookData.RequestBody)
$subscriptionId = "c36fb2f8-f98d-40d0-90a9-d65e93acb428"
$resourceGroup = "rg-crslbbiot-odin-dev"
$functionUrl = "https://telemetry-processor-odin-mt-poc.scm.azurewebsites.net"
$data.tenantId
$data.telemetryCollectionName
$data.token

# Define the authorization headers for REST API requests
$requestHeader = @{
  "Authorization" = "Bearer " + $data.token
}

$zipPath = "functionCode.zip"
$unzippedPath = "functionCode"
$newZipPath = "funcionCodeNew.zip"

# Download a zip file of the current azure function code
$functionCodeUri = "$functionUrl/api/zip/site/wwwroot/"
Invoke-RestMethod -Method 'Get' -Headers $requestheader -Uri $functionCodeUri -OutFile $zipPath

# Uncompress the azure function code
Expand-Archive -Path $zipPath -DestinationPath $unzippedPath

# Load the function.json file into a powershell json object
$functionJson = Get-Content -Path "$unzippedPath/telemetry-processor/function.json" | Out-String | ConvertFrom-Json

# Insert the new output binding into the existing function.json
$first8CharsOfTenantId = ($data.tenantId).SubString(0,8)
$newOutputBinding = @"
    {
      "type": "cosmosDB",
      "name": "outputTenant$($first8CharsOfTenantId)",
      "databaseName": "iot",
      "collectionName": "$($data.telemetryCollectionName)",
      "createIfNotExists": true,
      "connectionStringSetting": "cosmos-odin-mt-poc_DOCUMENTDB",
      "direction": "out",
      "partitionKey": "/temperature"
    }
"@
$functionJson.bindings += (ConvertFrom-Json -InputObject $newOutputBinding)

# Write the updated json object to the function.json file
ConvertTo-Json $functionJson | Out-File -FilePath "$unzippedPath/telemetry-processor/function.json"

# Compress the updated files back into a zip file
Compress-Archive -Path "$unzippedPath/*" -DestinationPath $newZipPath

# Update the Azure Function with the new zip file
$functionDeployUrl = "$functionUrl/api/zipdeploy"
$userAgent = "powershell/1.0"
Invoke-RestMethod -Method POST -Uri $functionDeployUrl -Headers $requestheader -UserAgent $userAgent  -InFile $newZipPath -ContentType "multipart/form-data"

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

# Write to table storage that the functions are updated for the tenant
"Trying to write to table storage"
$storageAccount = "functiondefinition"
$tableName = "tenant"
$table = Get-AzTableTable -resourceGroup $resourceGroup -tableName $tableName -storageAccountName $storageAccount
$row = Get-AzTableRowByPartitionKeyRowKey -Table $table -PartitionKey $data.tenantId[0] -RowKey $data.tenantId
$row.AreFunctionsUpdated = $true
$row | Update-AzTableRow -Table $table
"Done"