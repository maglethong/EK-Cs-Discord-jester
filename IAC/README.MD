# Instructions on terraform for setting um infrastructure on Azure

## Install Terraform

Install `terraform` tool

Install azure `AZ CLI` tool

On first time, run `terraform init -upgrade` in this folder to download the required providers

## Apply Infrastructure Changes

Run `terraform state pull` to recover the current state on Azure in case another Machine changed it

Run `terraform plan -out main.tfplan` to see changes that would be made when running terraform

Run `terraform apply main.tfplan`
