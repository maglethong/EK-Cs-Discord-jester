# Instructions on terraform for setting um infrastructure on Azure

## Install Terraform

Install `terraform` tool

Install azure `AZ CLI` tool

On first time,
1 - comment out the block `backend "azurerm"` in `providers.tf`, because it requires resources that are going to be created yet
2 - run `terraform init -upgrade` in this folder to download the required providers
3 - run `terraform apply` making sure all resources are created
4 - remove comments added on step 1
5 - run `terraform init` to store `providers.tf` on azure storage

## Apply Infrastructure Changes

Run `terraform apply` to apply the IAC changes
