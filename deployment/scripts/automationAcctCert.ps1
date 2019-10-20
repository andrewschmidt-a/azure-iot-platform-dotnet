# create a selfsigned cert
# Import the certificate to Automation account
# Import the cert to AD App registation
# Create a connection asset with cert thumbprint on Automation Account

param(
    [string] $accountName,
    [string] $resourceGroup,
    [string] $subscriptionId,
    [string] $tenantId,
    [string] $keyVaultName
)
function Import-Certificate() {
    $currDate = Get-Date
    $endDate = $currDate.AddYears(1)
    $notAfter = $endDate.AddYears(1)
    $certName = "AzureAutomation"
    $connectionName = "AzureRunAsConnection"
    $certificateDesc = "This certificate is used to authenticate with the service principal that was automatically created for this account. For details on this service principal and certificate, or to recreate them, go to this account’s Settings. For example usage, see the tutorial runbook in this account."
    $connectionDesc = "This connection contains information about the service principal that was automatically created for this automation account. For details on this service principal and its certificate, or to recreate them, go to this account’s Settings. For example usage, see the tutorial runbook in this account."
    $dnsName = "$accountName.azurewebsites.net"
    $certStore = "Cert:\LocalMachine\My"
    $certPassword = "o30+v0B1z3w§N"
    #$cert = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -DnsName $IdentityServerDns
    $cert = New-SelfSignedCertificate -DnsName $dnsName `
                                      -CertStoreLocation $CertStore `
                                      -KeyExportPolicy Exportable `
                                      -Provider "Microsoft Enhanced RSA and AES Cryptographic Provider" `
                                      -NotAfter $notAfter

    $pfxPassword = ConvertTo-SecureString -String $certPassword -Force -AsPlainText
    $pfxFilepath = "Cert:\LocalMachine\My\$($cert.Thumbprint)"
    Export-PfxCertificate -Cert $pfxFilepath -FilePath "$accountName.pfx" -Password $pfxPassword
    Import-PfxCertificate -Password $pfxPassword -FilePath "$accountName.pfx" -CertStoreLocation 'Cert:\LocalMachine\My'
    # convert to .cer
    Set-Content -Path "$accountName.cer" -Value ([Convert]::ToBase64String($cert.RawData)) -Encoding Ascii
    New-AzureRmAutomationCertificate -Name "AzureRunAsCertificate" `
                                     -Path "$accountName.pfx" `
                                     -Password $pfxPassword `
                                     -Description $certificateDesc `
                                     -AutomationAccountName $accountName `
                                     -ResourceGroupName $resourceGroup `
                                     -Exportable
    
    Write-Output "AzureRunAsCertificate has been added to $accountName"

    # adding the cert to Azure Keyvault 
    Write-Host "Adding Cert to Keyvault.."
    Import-AzureKeyVaultCertificate -VaultName $KeyVaultName `
                                    -Name $certName `
                                    -FilePath "$accountName.pfx" `
                                    -Password $pfxPassword 

    Write-Host Cert $certName added for $dnsName    
    
    # Populate the ConnectionFieldValues
    $applicationId = (Get-AzureKeyVaultSecret -VaultName $KeyVaultName -Name "ApplicationId").SecretValueText
    $connectionTypeName = "AzureServicePrincipal"
    $connectionFieldValues = @{ "ApplicationId" = $applicationId; 
                              "TenantId" = $tenantId; 
                              "CertificateThumbprint" = $cert.Thumbprint; 
                              "SubscriptionId" = $subscriptionId } 
    # Create a Automation connection asset named AzureRunAsConnection in the Automation account. This connection uses the service principal.
    Write-Output "Creating Connection in the Asset..."
    Remove-AzureRmAutomationConnection -ResourceGroupName $resourceGroup `
                                       -AutomationAccountName $accountName `
                                       -Name $connectionName `
                                       -Force `
                                       -ErrorAction SilentlyContinue

    New-AzureRmAutomationConnection -ResourceGroupName $resourceGroup `
                                    -AutomationAccountName $accountName `
                                    -Name $connectionName `
                                    -ConnectionTypeName $connectionTypeName `
                                    -ConnectionFieldValues $connectionFieldValues `
                                    -Description $connectionDesc

    # import the new cert to Azure AD app registration <Insufficient privilege>                              
    <#
    $cer = New-Object System.Security.Cryptography.X509Certificates.X509Certificate 
    $cer.Import("$accountName.cer") 
    $binCert = $cer.GetRawCertData() 
    $credValue = [System.Convert]::ToBase64String($binCert)
    New-AzureRmADAppCredential -ApplicationId $applicationId `
                               -CertValue $credValue `
                               -StartDate $cer.GetEffectiveDateString() `
                               -EndDate $cer.GetExpirationDateString()
    #>
}

# check if cert exists or expired 
$result = Get-AzureRmAutomationCertificate -ResourceGroupName $resourceGroup -AutomationAccountName $accountName

if ([string]::IsNullOrEmpty($result)){
    Write-Output "Cert does not exist, creating one"
    #Create a new cert
    Import-Certificate
}
else {
    #$expDate = $thumbprint.ExpiryTime | Where-Object $_.Name -eq "AzureRunAsCertificate"
    #$expDt = $thumprint.expiryTime | Where-Object { $thumprint.Name -eq "AzureRunAsCertificate" }
    $certdetails = $result | Select-Object | Where-Object {$_.Name -eq "AzureRunAsCertificate"}
    $thumprint = $certdetails.Thumbprint
    $expDate = $certdetails.ExpiryTime.DateTime
    if ( $currDate -gt $expDate)
        { Write-Output "Cert expired, creating one" $thumprint }
        #Create a new cert
        Import-Certificate
    else { Write-Output "Cert still active " $thumprint }
}