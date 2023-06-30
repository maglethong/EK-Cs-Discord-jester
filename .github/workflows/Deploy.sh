﻿#! /bin/bash

echo "$(date) --- Start"

##################
# Install docker #
##################
# See https://docs.docker.com/engine/install/debian/

# Update the apt package index and install packages to allow apt to use a repository over HTTPS:
apt-get update
apt-get install -y ca-certificates curl gnupg

# Add Docker’s official GPG key:
install -m 0755 -d /etc/apt/keyrings
rm -f /etc/apt/keyrings/docker.gpg
curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg

# set up the repository
echo \
  "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian \
  "$(. /etc/os-release && echo "$VERSION_CODENAME")" stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

# Install Docker Engine, containerd, and Docker Compose.
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

######################
# Install Powershell #
######################
# See: https://learn.microsoft.com/en-us/powershell/scripting/install/install-debian?view=powershell-7.3

# Download the Microsoft repository GPG keys
rm packages-microsoft-prod.deb -f
wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb

# Register the Microsoft repository GPG keys
dpkg -i packages-microsoft-prod.deb

# Update the list of products
apt-get update

# Install PowerShell
apt-get install -y powershell

# cleanup
rm packages-microsoft-prod.deb

###################
## Install az cli #
###################
##  see: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-linux?pivots=apt
#
## Get packages needed for the install process:
#apt-get update
#apt-get install -y ca-certificates curl apt-transport-https lsb-release gnupg
#
## Download and install the Microsoft signing key:
#rm -f /etc/apt/keyrings/microsoft.gpg
#curl -sLS https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | tee /etc/apt/keyrings/microsoft.gpg > /dev/null
#chmod go+r /etc/apt/keyrings/microsoft.gpg
#
## Add the Azure CLI software repository:
#AZ_REPO=$(lsb_release -cs)
#echo "deb [arch=`dpkg --print-architecture` signed-by=/etc/apt/keyrings/microsoft.gpg] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" |
#    tee /etc/apt/sources.list.d/azure-cli.list
#    
## Update repository information and install the azure-cli package:
#sudo apt-get update
#sudo apt-get install -y azure-cli


#######################
# Start actual deploy #
#######################

AZURE_BLOB_CONTAINER_URL=$1
IMAGE_VERSION=$2
echo "$3" > creds.json

AZURE_CLIENT_ID=$(powershell -command "(cat creds.json |  ConvertFrom-Json).clientId")
AZURE_TENANT_ID=$(powershell -command "(cat creds.json |  ConvertFrom-Json).clientId")
AZURE_CLIENT_SECRET=$(powershell -command "(cat creds.json |  ConvertFrom-Json).clientId")
rm creds.json

# shellcheck disable=SC2129
echo "AZURE_BLOB_CONTAINER_URL: $AZURE_BLOB_CONTAINER_URL"
echo "IMAGE_VERSION: $IMAGE_VERSION"
echo "AZURE_CLIENT_ID: $AZURE_CLIENT_ID"
echo "AZURE_TENANT_ID: $AZURE_TENANT_ID"
echo "AZURE_CLIENT_SECRET: *****"

# Stop and cleanup all old image
docker stop ek-discord-jester
docker image tag "maglethong/ek/discord/jester:latest" "maglethong/ek/discord/jester:backup" 

# Import new image
FILE_NAME="DockerImage-${IMAGE_VERSION}.tar"
rm -f $FILE_NAME
wget "${AZURE_BLOB_CONTAINER_URL}${FILE_NAME}"
docker load --input $FILE_NAME
rm $FILE_NAME
docker image tag "maglethong/ek/discord/jester:${IMAGE_VERSION}" "maglethong/ek/discord/jester:latest" 
docker image rm "maglethong/ek/discord/jester:${IMAGE_VERSION}"

# Start new image
docker run \
    --rm \
    -d \
    --name ek-discord-jester \
    -p 80:80 \
    -e AZURE_CLIENT_ID="$AZURE_CLIENT_ID" \
    -e AZURE_TENANT_ID="$AZURE_TENANT_ID" \
    -e AZURE_CLIENT_SECRET="$AZURE_CLIENT_SECRET" \
    "maglethong/ek/discord/jester:latest" 
    

# remove image backup
docker image rm "maglethong/ek/discord/jester:backup" 

echo "$(date) --- Finished"
echo "================================================================================================="

# TODO:
# [] Use a docker compose file
# [x] Use Service principal to connect to Azure and fetch. Secrets will be fetched from there then
# [] Add automatic rollback
