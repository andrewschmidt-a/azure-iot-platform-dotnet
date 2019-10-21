# create and import a runbook from a file path
# publish the runbook 
# create the webhook
# store the webhook in the keyvault

param(
    [string] $accountName,
    [string] $resourceGroup,
    [string] $scriptFolder,
    [string] $keyvaultName
)

$currtime = Get-Date
$expDate = $currtime.AddDays(365)

function importRunbook($runbookName, $filepath) {
    
    Import-AzureRMAutomationRunbook -Name $runbookName -Path $filepath `
                                    -ResourceGroupName $resourceGroup -AutomationAccountName $accountName `
                                    -Type PowerShell `
                                    -Force

    Publish-AzureRmAutomationRunbook -Name $runbookName -AutomationAccountName $accountName `
                                     -ResourceGroupName $resourceGroup
}

function createWebhook($webhook, $runbookName, $expDate, $secretName) {

    $ifExists = Get-AzureRmAutomationWebhook -RunbookName $runbookName `
                                             -ResourceGroupName $resourceGroup `
                                             -AutomationAccountName $accountName

    if ([string]::IsNullOrEmpty($ifExists)) {
        $result = New-AzureRmAutomationWebhook -Name $webhook -RunbookName $runbookName `
                                               -ExpiryTime $expDate -ResourceGroup $resourceGroup `
                                               -AutomationAccountName $accountName `
                                               -IsEnabled $True `
                                               -Force
       
       # add the webhook to keyvault
       $webookUri = $result.WebhookURI
       addtoKeyvault -webookUri $webookUri -secretName $secretName    
       Write-Output "webhook added to keyvault, $secretName" 
    }
    else{
        Write-Output "webhook already exists for the runbook, $runbookName"
    }
}

function addtoKeyvault($webookUri, $secretName ){
    $vaultwebookUri = ConvertTo-SecureString -String $webookUri -AsPlainText -Force
    Set-AzureKeyVaultSecret -VaultName $keyvaultName -Name $secretName -SecretValue $vaultwebookUri
}

# import the runbook with code
importRunbook -runbookName "CreateIoTHubTenant" -filepath "$scriptFolder\CreateIoThubRunbook.ps1" 
importRunbook -runbookName "DeleteIoTHubTenant" -filepath "$scriptFolder\DeleteIoThubRunbook.ps1"

# create the webhook and store to the Keyvault
createWebhook -webhook "Tenantcreatewebhook" -runbookName "CreateIoTHubTenant" -expDate $expDate -secretName "createIotHubWebHookUrl"
createWebhook -webhook "Tenantdeleteewebhook" -runbookName "DeleteIoTHubTenant" -expDate $expDate -secretName "deleteIotHubWebHookUrl"

# end 
