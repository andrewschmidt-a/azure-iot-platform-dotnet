<#
    .DESCRIPTION
        A run book for creating a new IoT Hub for a tenant

    .NOTES
        AUTHOR: Nate Oelke
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

# Define template for creating IoT Hub
$iotHubTemplate = @"
{  
  "name":"$($data.iotHubName)",
  "type":"Microsoft.Devices/IotHubs",
  "location":"$($data.location)",
  "sku":{  
    "name":"S1",
    "tier":"Standard",
    "capacity":1
  },
	"properties": {
		"state": "Active",
		"routing":{  
			"enrichments":[  
				{  
					"key":"tenant",
					"value":"$($data.tenantId)",
					"endpointNames":[  
						"event-hub-telemetry", "event-hub-twin-change", "event-hub-lifecycle" 
					]
				}
			],
			"endpoints":{  
				"serviceBusQueues":[  

				],
				"serviceBusTopics":[  

				],
				"eventHubs":[  
					{
            "connectionString": "$($data.telemetryEventHubConnString)",
            "name": "event-hub-telemetry",
            "subscriptionId": "$($data.subscriptionId)",
            "resourceGroup": "$($data.resourceGroup)"
          },
					{
            "connectionString": "$($data.twinChangeEventHubConnString)",
            "name": "event-hub-twin-change",
            "subscriptionId": "$($data.subscriptionId)",
            "resourceGroup": "$($data.resourceGroup)"
          },
					{
            "connectionString": "$($data.lifecycleEventHubConnString)",
            "name": "event-hub-lifecycle",
            "subscriptionId": "$($data.subscriptionId)",
            "resourceGroup": "$($data.resourceGroup)"
          }
				],
				"storageContainers":[  

				]
			},
			"routes":[  
				{  
					"name": "telemetry",
					"source":"DeviceMessages",
					"condition":"true",
					"endpointNames":[  
						"event-hub-telemetry"
					],
					"isEnabled":true
				},
				{  
					"name": "lifecycle",
					"source":"DeviceLifecycleEvents",
					"condition":"true",
					"endpointNames":[  
						"event-hub-lifecycle"
					],
					"isEnabled":true
				},
				{  
					"name": "twin-change",
					"source":"TwinChangeEvents",
					"condition":"true",
					"endpointNames":[  
						"event-hub-twin-change"
					],
					"isEnabled":true
				}
			]
		}
	}
}
"@

$requestHeader = @{
  "Authorization" = "Bearer " + $data.token
  "Content-Type" = "application/json"
}

$iotHubUri = "https://management.azure.com/subscriptions/$($data.subscriptionId)/resourceGroups/$($data.resourceGroup)/providers/Microsoft.Devices/IotHubs/$($data.iotHubName)?api-version=2019-03-22-preview"

# Create IoT Hub using Azure REST API
$result = (Invoke-RestMethod -Method Put -Headers $requestheader -Uri $iotHubUri -Body $iotHubTemplate)

# Wait for IoT Hub to be created
$tries = 0
while (($result.properties.state -ne "Active") -and ($tries -lt 30)) {
   Start-Sleep -Second 15
   $result = (Invoke-RestMethod -Method Get -Headers $requestheader -Uri $iotHubUri)
   $tries++
}

# Load the connection string
$policy = "iothubowner" 
$iotHubKeysUri = "https://management.azure.com/subscriptions/$($data.subscriptionId)/resourceGroups/$($data.resourceGroup)/providers/Microsoft.Devices/IotHubs/$($data.iotHubName)/IotHubKeys/$policy/listkeys?api-version=2019-03-22-preview"
$result = (Invoke-RestMethod -Method Post -Headers $requestheader -Uri $iotHubKeysUri)
$result

# Create the connection string
$sharedAccessKey = $result.primaryKey
$connectionString = "HostName=$($data.iotHubName).azure-devices.net;SharedAccessKeyName=$policy;SharedAccessKey=$sharedAccessKey"

# Write the IoT Hub connection string to app config
$requestHeader = @{
  "Content-Type" = "application/json"
}
$appConfigKey = "tenant:$($data.tenantId):iotHubConnectionString"
$appConfigBody = @"
{
     connectionstring : "$appConfigConnectionString", name : "$appConfigKey", value : "$connectionString"
}
"@

$setAppConfigEndpoint
$result = (Invoke-RestMethod -Method Post -Headers $requestheader -Uri $setAppConfigEndpoint -Body $appConfigBody)

# Write to table storage
"Trying to write to table storage"
$storageAccount = "functiondefinition"
$tableName = "tenant"
$table = Get-AzTableTable -resourceGroup $data.resourceGroup -tableName $tableName -storageAccountName $storageAccount
$row = Get-AzTableRowByPartitionKeyRowKey -Table $table -PartitionKey $data.tenantId[0] -RowKey $data.tenantId
$row.IsIotHubDeployed = $true
$row.IotHubConnectionString = $connectionString
$row | Update-AzTableRow -Table $table
"Done"