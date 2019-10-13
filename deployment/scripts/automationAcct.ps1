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

$automationAccountName = $accountName
$scriptFolder = $scriptFolder
$RGName = $resourceGroup
$vaultName = $keyvaultName

function importRunbook($runbookName, $filepath) {

    Import-AzureRMAutomationRunbook -Name $runbookName -Path $filepath `
                                    -ResourceGroupName $RGName -AutomationAccountName $automationAccountName `
                                    -Type PowerShell

    Publish-AzureRmAutomationRunbook -Name $runbookName -AutomationAccountName $automationAccountName `
                                     -ResourceGroupName $RGName
}

function createWebhook($webhook, $runbookName, $expDate) {

    $ifExists = Get-AzureRmAutomationWebhook -RunbookName $runbookName `
                                             -ResourceGroupName $RGName `
                                             -AutomationAccountName $automationAccountName

    if ([string]::IsNullOrEmpty($ifExists)) {
        New-AzureRmAutomationWebhook -Name $webhook -RunbookName $runbookName `
                                     -ExpiryTime $expDate -ResourceGroup $RGName `
                                     -AutomationAccountName $automationAccountName `
                                     -IsEnabled $True -Force                                
    }
    else{
        Write-Output "webhook exists for the runbook, $runbookName"
    }
}

# import the runbook with code
$result = importRunbook -runbookName "CreateIoTHubTenant" -filepath "$scriptFolder\CreateIoThubRunbook.ps1"
Write-Output $result
$result = importRunbook -runbookName "DeleteIoTHubTenant" -filepath "$scriptFolder\DeleteIoThubRunbook.ps1"
Write-Output $result

# create the webhook and store to the Keyvault
$result = createWebhook -webhook "Tenantcreatewebhook" -runbookName "CreateIoTHubTenant" -expDate $expDate
$createwebookUri = $result.WebhookURI
$vaultwebookUri = ConvertTo-SecureString -String $createwebookUri -AsPlainText -Force
Set-AzureKeyVaultSecret -VaultName $vaultName -Name "createIotHubWebHookUrl" -SecretValue $vaultwebookUri

$result = createWebhook -webhook "Tenantdeleteewebhook" -runbookName "DeleteIoTHubTenant" -expDate $expDate
$deletewebookUri = $result.WebhookURI
$vaultwebookUri = ConvertTo-SecureString -String $deletewebookUri -AsPlainText -Force
Set-AzureKeyVaultSecret -VaultName $vaultName -Name "deleteIotHubWebHookUrl" -SecretValue $vaultwebookUri

# end 


