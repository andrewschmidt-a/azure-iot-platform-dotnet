#!/bin/bash
az aks get-credentials -n $1 -g $2

n=0
until [ $n -ge 5 ]
do
# Public IP address of your ingress controller
    IP=$(kubectl get service -l app=nginx-ingress -l component=controller --namespace ingress-basic -o json | jq -r .items[0].status.loadBalancer.ingress[0].ip)
    if [[ ! -z "$IP" ]]
        then echo $IP
        break
    fi 
  n=$[$n+1]
  sleep 30
done

# Name to associate with public IP address
DNSNAME=${1,,} # Get from Env Variable
# Get the resource-id of the public ip
PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '$IP')].[id]" --output tsv)
# Update public ip address with DNS name
az network public-ip update --ids $PUBLICIPID --dns-name $DNSNAME