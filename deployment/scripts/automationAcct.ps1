# create and import a runbook from a file path
# publish the runbook 

$currtime = Get-Date
$expDate = $currtime.AddDays(365)

$automationAccountName = "crsl-automationAccount"
$runbookName = "CreateIoTHubTenant"
$scriptFolder = "C:\Users\A84qhzz\Downloads"
$RGName = "radhatest-cosmos"
$webhook1 = "Tenantcreatewebhook"

Import-AzureRMAutomationRunbook -Name $runbookName -Path "$scriptFolder\CreateIoThubRunbook.ps1" `
                                -ResourceGroupName $RGName -AutomationAccountName $automationAccountName `
                                -Type PowerShell

Publish-AzureRmAutomationRunbook -Name $runbookName -AutomationAccountName $automationAccountName `
                                 -ResourceGroupName $RGName  

$webhook = New-AzureRmAutomationWebhook -Name $webhook1 -RunbookName $runbookName `
                                        -ExpiryTime $expDate -ResourceGroup $RGName `
                                        -AutomationAccountName $automationAccountName `
                                        -IsEnabled $True -Force

Write-Host "webhook Uri: $webhook.WebhookURI"

