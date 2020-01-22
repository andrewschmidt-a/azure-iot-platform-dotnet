param (
    [string]$1 = ""
 )

az pipelines build queue --organization https://dev.azure.com/3M-Bluebird/ --project AzurePlatform --definition-name $1