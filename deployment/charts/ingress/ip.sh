#!/bin/bash
az aks get-credentials -n $1 -g $2
# Public IP address of your ingress controller

n=0
until [ $n -ge 5 ]
do
   IP=$(kubectl get service -l app=nginx-ingress -l component=controller --namespace ingress-basic -o json | jq -r .items[0].status.loadBalancer.ingress[0].ip) \
   2>&1 && break
  n=$[$n+1]
  sleep 15
done

if [ $n -ge 5]
then
  echo "Error Deploying ingress controller" 1>&2
  exit 64
fi

echo $IP
# Name to associate with public IP address
DNSNAME=${1,,} # Get from Env Variable
# Get the resource-id of the public ip
PUBLICIPID=$(az network public-ip list --query "[?ipAddress!=null]|[?contains(ipAddress, '$IP')].[id]" --output tsv)
# Update public ip address with DNS name
az network public-ip update --ids $PUBLICIPID --dns-name $DNSNAME