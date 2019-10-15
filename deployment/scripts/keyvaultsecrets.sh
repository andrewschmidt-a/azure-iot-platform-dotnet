#!/bin/bash
#*********************************
# Author: Momodou Jaiteh         *
# 3M CRSL - IoT Platform         *
# KeyVault Keys Setup Script     *
#*********************************
#Troubleshoot azure cli and bash versions
az --version
echo $TERM
echo $SHELL

# Command parsing
while [[ $# -gt 0 ]] ; do 
	case "$1" in
		"--azureSubscription") cli_azureSubscription=$2
        ;;
		"--resourceGroup") cli_resourceGroup=$2
        ;;
		"--tenant") cli_tenant=$2
		;;
		"--ClientSecret") cli_ClientSecret=$2
		;;
		"--ApplicationId") cli_ApplicationId=$2
		;;
	esac
       shift
done

echo $cli_azureSubscription
echo $cli_resourceGroup
echo $cli_tenant
echo $cli_ClientSecret
echo $cli_ApplicationId

# Login to Azure portal with Service Principal Credentials
az login --service-principal -u  "${cli_ApplicationId}" -p "${cli_ClientSecret}" --tenant "${cli_tenant}"

# Set the right subscriptions
az account set --subscription "$cli_azureSubscription"

# Set correct resource group, sql server userid and password
rg=${cli_resourceGroup}
ApplicationId=${cli_ApplicationId}

#***********************************************************
# THESE DON'T OFTEN CHANGE AND ARE THEREFORE HARDCODED HERE*
#***********************************************************

#--------------------------------------------------------------------
myEmailSMTPHost='smtp.sendgrid.net'
myEmailSMTPUserId='azure_67748810fea7a7ec9d0b8213a9779967@azure.com'
myEmailSMTPPassword='8eorX2vsqob6p15'
myEmailSendGridApiKey='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'

#---------------------------------------------------------------------

#***************************************************
# AUTO GENERATED - DO NOT CHANGE THESE VALUES BELOW*
#***************************************************

# Initial KeyVault Setup

# tenantId=$(az account list| grep -i tenantId | cut -d "\"" -f4 | uniq)
echo "The Tenant Id is: $cli_tenant"

kVaultName=$(az keyvault list --resource-group ${rg}  | grep -i name |grep 'vlt' | awk -F "\"" '{print $4}')
echo "The KeyVault Name is:  ${kVaultName}"

kvlocation=$(az keyvault list --resource-group  ${rg} | grep -i location | awk -F "\"" '{print $4}')
echo "The KeyVault Location is:  ${kvlocation}"

# Setting the Variables for the the KeyVault

# Get the blob storage account name: 
blobStorageAccountName=$(az storage account list -g $rg | grep name | grep storage | awk -F "\"" '{print $4}')

#Get the blob storage  key, we only need one: 
blobStorageKey=$(az storage account keys list -g $rg -n $blobStorageAccountName  | grep value | head -n1 | awk -F "\"" '{print $4}')

# build the string for the blob storage connection 
blobStorageConnString="DefaultEndpointsProtocol=https;AccountName=${blobStorageAccountName};AccountKey=${blobStorageKey};EndpointSuffix=core.windows.net"

# build the Cosmos db connection 
myCosmosDbAccountName=$(az cosmosdb list --resource-group $rg | grep name |  awk -F "\"" '{print $4}')
myCosmosDBUri="https://${myCosmosDbAccountName}.documents.azure.com:443/"
myDocDBAuthKey=$(az cosmosdb list-keys --resource-group $rg --name ${myCosmosDbAccountName} | grep primaryMasterKey |  awk -F "\"" '{print $4}')
myCosmosDBConnection="AccountEndpoint=$(myCosmosDBUri);AccountKey=$(myDocDBAuthKey);"

# Adding EventHub Connection 
myEvNamespace=$(az eventhubs namespace list --resource-group $rg| grep name| grep eventhubs| awk -F "\"" '{print $4}'| cut -d "/" -f1)
ehQueueAuthRule='iothubs'

lifecycleEv=$(az eventhubs eventhub list --resource-group $rg --namespace-name $myEvNamespace| grep name |grep lifecycle| awk -F "\"" '{print $4}'| cut -d "/" -f1)
lifecycleEvConn=$(az eventhubs eventhub authorization-rule keys list --resource-group $rg --namespace-name $myEvNamespace --eventhub-name $lifecycleEv --name $ehQueueAuthRule |grep primaryConnectionString | awk -F "\"" '{print $4}')

telemetryEv=$(az eventhubs eventhub list --resource-group $rg --namespace-name $myEvNamespace| grep name |grep telemetry| awk -F "\"" '{print $4}'| cut -d "/" -f1)
telemetryEvConn=$(az eventhubs eventhub authorization-rule keys list --resource-group $rg --namespace-name $myEvNamespace --eventhub-name $telemetryEv --name $ehQueueAuthRule |grep primaryConnectionString | awk -F "\"" '{print $4}')

twinchangeEv=$(az eventhubs eventhub list --resource-group $rg --namespace-name $myEvNamespace| grep name |grep twin-change| awk -F "\"" '{print $4}'| cut -d "/" -f1)
twinchangeEvConn=$(az eventhubs eventhub authorization-rule keys list --resource-group $rg --namespace-name $myEvNamespace --eventhub-name $twinchangeEv --name $ehQueueAuthRule |grep primaryConnectionString | awk -F "\"" '{print $4}')

# Azure Maps Key
mapsacct=$(az maps account list --resource-group $rg| grep name | grep map | awk -F "\"" '{print $4}')
mapskey=$(az maps account keys list --name $mapsacct --resource-group $rg | grep primaryKey | awk -F "\"" '{print $4}')

# Adding Keys to the KeyVault
az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'documentDBAuthKey' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'documentDBAuthKey'  --value "${myDocDBAuthKey}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'documentDBEndpoint' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'documentDBEndpoint'  --value "${myCosmosDBUri}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'documentDBConnectionString' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'documentDBConnectionString'  --value   "${myCosmosDBConnection}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'EmailSMTPHost' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'EmailSMTPHost'  --value "${myEmailSMTPHost}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'EmailSMTPUserId' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'EmailSMTPUserId'  --value "${myEmailSMTPUserId}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'EmailSMTPPassword' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'EmailSMTPPassword'  --value "${myEmailSMTPPassword}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'sendGridAPIKey' 2>/dev/null
az keyvault secret set --vault-name  $kVaultName --name 'sendGridAPIKey'  --value "${myEmailSendGridApiKey}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'ApplicationId' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'ApplicationId' --value "${ApplicationId}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'ClientSecret' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'ClientSecret' --value "${cli_ClientSecret}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'storageAccountConnectionString' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'storageAccountConnectionString' --value "${blobStorageConnString}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'LifecycleEventHubConnection' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'LifecycleEventHubConnection' --value "${lifecycleEvConn}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'TelemetryEventHubConnection' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'TelemetryEventHubConnection' --value "${telemetryEvConn}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'TwinChangeEventHubConnection' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'TwinChangeEventHubConnection' --value "${twinchangeEvConn}"

az keyvault secret set-attributes --enable false --vault-name $kVaultName --name 'azureMapsKey' 2>/dev/null
az keyvault secret set --vault-name $kVaultName --name 'azureMapsKey' --value "${mapskey}"

# end