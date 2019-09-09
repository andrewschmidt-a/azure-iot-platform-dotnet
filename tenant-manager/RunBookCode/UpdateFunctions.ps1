<#
    .DESCRIPTION
        A run book for adding a new Cosmos DB output to the 3 Azure Functions (Telemetry, Lifecycle, and Twin Change)

    .NOTES
        AUTHOR: Nate Oelke
                Sean Dubiel
#>

param
(
    [Parameter (Mandatory = $false)]
    [object] $WebhookData
)

# Record start time
$startTime = Get-Date

# If runbook was called from Webhook, WebhookData will not be null.
if (-Not $WebhookData) {
    # Error
    write-Error "This runbook is meant to be started from a webhook only."
}

# Authenticate with the service principle so that we can write to table storage
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
$data.token

# Get the automation account name and runbook name
$currentJobId= $PSPrivateMetadata.JobId.Guid
$job = Get-AzAutomationJob -ResourceGroupName $data.resourceGroup -AutomationAccountName $data.automationAccountName -Id $currentJobId `
    -ErrorAction SilentlyContinue

if ($job) 
{
    $runbookName = $job.RunbookName
}

# Wait for any run books of this type that started earlier to finish before starting
$clearedToContinue = 0
while ($clearedToContinue -eq 0) {
    $clearedToContinue = 1
    $currentRunningJobs = Get-AzAutomationJob -AutomationAccountName $data.automationAccountName -ResourceGroupName $data.resourceGroup `
        -Status "Running" -RunbookName $runbookName
    foreach ($job in $currentRunningJobs) {
        if ($job.JobId.tostring() -ne $currentJobId -And $job.StartTime -lt $startTime) {
            "Already running job detected. Sleeping for 30 seconds"
            $clearedToContinue = 0
            Start-Sleep -Second 30
            break
        }
    }
}

    # Define the authorization headers for REST API requests
$requestHeader = @{
    "Authorization" = "Bearer " + $data.token
}
$data.type
if($data.type -eq "create"){
    "create"

    # Define a function for adding a new Cosmos output to an azure function
    function Add-CosmosOutputToFunction {
        Param ($requestHeader, $tenantId, $functionUrl, $functionName, $databaseName,
            $cosmosCollectionName, $cosmosConnectionSetting, $cosmosPartitionKey)

        $zipPath = "$functionName.zip"
        $unzippedPath = "$functionName"
        $newZipPath = "$($functionName)New.zip"

        # Download a zip file of the current azure function code
        $functionCodeUri = "$functionUrl/api/zip/site/wwwroot/"
        $functionCodeUri
        Invoke-RestMethod -Method 'Get' -Headers $requestheader -Uri $functionCodeUri -OutFile $zipPath

        # Uncompress the azure function code
        Expand-Archive -Path $zipPath -DestinationPath $unzippedPath

        # Load the function.json file into a powershell json object
        $functionJson = Get-Content -Path "$unzippedPath/$functionName/function.json" | Out-String | ConvertFrom-Json

        # Insert the new output binding into the existing function.json
        $newOutputBinding = @"
            {
            "type": "cosmosDB",
            "name": "outputTenant$tenantId",
            "databaseName": "$($data.databaseName)",
            "collectionName": "$cosmosCollectionName",
            "createIfNotExists": true,
            "connectionStringSetting": "$cosmosConnectionSetting",
            "direction": "out",
            "partitionKey": "$cosmosPartitionKey"
            }
"@ # This is not indented due to powershell wanting the end of a here-string with no whitespace before it
        
        $functionJson.bindings += (ConvertFrom-Json -InputObject $newOutputBinding)

        # Write the updated json object to the function.json file
        ConvertTo-Json $functionJson | Out-File -FilePath "$unzippedPath/$functionName/function.json"

        # Compress the updated files back into a zip file
        Compress-Archive -Path "$unzippedPath/*" -DestinationPath $newZipPath

        # Update the Azure Function with the new zip file
        $functionDeployUrl = "$functionUrl/api/zipdeploy"
        $userAgent = "powershell/1.0"
        Invoke-RestMethod -Method POST -Uri $functionDeployUrl -Headers $requestheader -UserAgent $userAgent  -InFile $newZipPath -ContentType "multipart/form-data"
    }

    # Update the telemetry function
    Add-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.telemetryFunctionUrl -functionName $data.telemetryFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.telemetryCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"

    # Update the twin change function
    Add-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.twinChangeFunctionUrl -functionName $data.twinChangeFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.twinChangeCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"

    # Update the lifecycle function
    Add-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.lifecycleFunctionUrl -functionName $data.lifecycleFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.lifecycleCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"

    # Write to table storage that the functions are updated for the tenant
    "Trying to write to table storage"
    $table = Get-AzTableTable -resourceGroup $data.resourceGroup -tableName $data.tableName -storageAccountName $data.storageAccount
    $row = Get-AzTableRowByPartitionKeyRowKey -Table $table -PartitionKey $data.tenantId[0] -RowKey $data.tenantId
    $row.AreFunctionsUpdated = $true
    $row | Update-AzTableRow -Table $table
    "Done"
}

if($data.type -eq "delete"){
# Define a function for deleting a new Cosmos output to an azure function
    function Remove-CosmosOutputToFunction {
        Param ($requestHeader, $tenantId, $functionUrl, $functionName, $databaseName,
            $cosmosCollectionName, $cosmosConnectionSetting, $cosmosPartitionKey)

        $zipPath = "$functionName.zip"
        $unzippedPath = "$functionName"
        $newZipPath = "$($functionName)New.zip"

        # Download a zip file of the current azure function code
        $functionCodeUri = "$functionUrl/api/zip/site/wwwroot/"
        $functionCodeUri
        Invoke-RestMethod -Method 'Get' -Headers $requestheader -Uri $functionCodeUri -OutFile $zipPath

        # Uncompress the azure function code
        Expand-Archive -Path $zipPath -DestinationPath $unzippedPath

        # Load the function.json file into a powershell json object
        $functionJson = Get-Content -Path "$unzippedPath/$functionName/function.json" | Out-String | ConvertFrom-Json

        # Gather the new output binding into the existing function.json
        $first8CharsOfTenantId = ($tenantId).SubString(0,8)
        $newOutputBinding = @"
            {
            "type": "cosmosDB",
            "name": "outputTenant$first8CharsOfTenantId",
            "databaseName": "$($data.databaseName)",
            "collectionName": "$cosmosCollectionName",
            "createIfNotExists": true,
            "connectionStringSetting": "$cosmosConnectionSetting",
            "direction": "out",
            "partitionKey": "$cosmosPartitionKey"
            }
"@ # This is not indented due to powershell wanting the end of a here-string with no whitespace before it

        $functionJson.bindings = $functionJson.bindings | Select-Object * -ExcludeProperty "outputTenant$first8CharsOfTenantId"

        # Write the updated json object to the function.json file
        ConvertTo-Json $functionJson | Out-File -FilePath "$unzippedPath/$functionName/function.json"

        # Compress the updated files back into a zip file
        Compress-Archive -Path "$unzippedPath/*" -DestinationPath $newZipPath

        # Update the Azure Function with the new zip file
        $functionDeployUrl = "$functionUrl/api/zipdeploy"
        $userAgent = "powershell/1.0"
        Invoke-RestMethod -Method POST -Uri $functionDeployUrl -Headers $requestheader -UserAgent $userAgent  -InFile $newZipPath -ContentType "multipart/form-data"
    }

    # Update the telemetry function
    Remove-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.telemetryFunctionUrl -functionName $data.telemetryFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.telemetryCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"

    # Update the twin change function
    Remove-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.twinChangeFunctionUrl -functionName $data.twinChangeFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.twinChangeCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"

    # Update the lifecycle function
    Remove-CosmosOutputToFunction -requestHeader $requestHeader -tenantId $data.tenantId `
        -functionUrl $data.lifecycleFunctionUrl -functionName $data.lifecycleFunctionName `
        -databaseName $data.databaseName -cosmosCollectionName $data.lifecycleCollectionName `
        -cosmosConnectionSetting $cosmosConnectionSetting -cosmosPartitionKey "/_deviceId"
}

"Done"
