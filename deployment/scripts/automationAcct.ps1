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

function addModulefromGallery($moduleName){
    # import AzureRM.Storage from PS Gallery
    $galleryRepoUri = (find-module -Name AzureRM.Storage).RepositorySourceLocation
    $moduleUri = '{0}{1}' -f $galleryRepoUri, [string]::Concat('/package/',$moduleName)
    Write-Output "Importing $moduleName from $moduleUri"
    New-AzureRmAutomationModule -ResourceGroupName $resourceGroup `
                                -AutomationAccountName $accountName `
                                -Name $moduleName `
                                -ContentLink $moduleUri

    Write-Output "Module $moduleName added to Automation Acct .."
}

function addtoKeyvault($webookUri, $secretName ){
    $vaultwebookUri = ConvertTo-SecureString -String $webookUri -AsPlainText -Force
    Set-AzureKeyVaultSecret -VaultName $keyvaultName -Name $secretName -SecretValue $vaultwebookUri
}

# import modules from PS Gallery
Write-Output "Adding modules to Automation Acct .."
addModulefromGallery -moduleName "Az.Storage"
addModulefromGallery -moduleName "Az.Accounts"
addModulefromGallery -moduleName "Az.DeviceProvisioningServices"

# import the runbook with code with the Path specified
Write-Output "Importing modules to Automation Acct .."
importRunbook -runbookName "CreateIoTHubTenant" -filepath "$scriptFolder\CreateIoTHub.ps1" 
importRunbook -runbookName "DeleteIoTHubTenant" -filepath "$scriptFolder\DeleteIoTHub.ps1"

# create the webhook and store to the Keyvault
createWebhook -webhook "CreateIotHub" -runbookName "CreateIoTHubTenant" -expDate $expDate -secretName "createIotHubWebHookUrl"
createWebhook -webhook "DeleteIotHub" -runbookName "DeleteIoTHubTenant" -expDate $expDate -secretName "deleteIotHubWebHookUrl"

# end 
