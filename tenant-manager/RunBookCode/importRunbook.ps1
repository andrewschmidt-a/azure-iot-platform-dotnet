$automationAccountName =  "crsliot-automation-wkbnch"
$runbookName = "CreateIoTHubTenant"
$scriptPath = "./CreateIoTHubTenant.ps1"
$RGName = "rg-iot-crsl-wkbnch"

Import-AzureRMAutomationRunbook -Name $runbookName -Path $scriptPath `
-ResourceGroupName $RGName -AutomationAccountName $automationAccountName `
-Type PowerShellWorkflow -Force