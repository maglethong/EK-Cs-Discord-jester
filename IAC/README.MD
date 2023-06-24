# Instructions on terraform for setting um infrastructure on Azure

## Install Terraform

1. Install `terraform` tool
2. Install azure `AZ CLI` tool
3. [Optional] Add a `terraform.tfvars.json` file with the following content:

    ```json
    {
     "resource_group_name": "rg-ek",
     "host_vm_name": "mv-myvm",
     "host_vm": {
       "create": true,
       "enable_ssh": false,
       "size": "Standard_B1ls"
     },
     "vault_name": "kv-ek-discord",
     "vm_admin_user": "azureuser",
     "ssh_private_key_secret_name": "kv-ek-my-vm-ssh-priv",
     "vm_ip_secret_name": "kv-ek-my-vm-ip"
    }
    ```
       
    Adjust it as desired. This file is used by `.ps1` utility scripts.

    **IMPORTANT:** it is good practice leaving ssh ports closed when not in use to avoid ssh attacks.
    Enable `host_vm.enable_ssh = true` only when maintaining the VM.


4. comment out the block `backend "azurerm"` in `providers.tf`, because it requires resources that are going to be created yet
5. run `terraform init -upgrade` in this folder to download the required providers
6. run `terraform apply` making sure all resources are created
7. remove comments added on step 4
8. run `terraform init` to store `providers.tf` on azure storage

## Apply Infrastructure Changes

Run `terraform apply` to apply the IAC changes

Price estimated for all created resources is `~6.6$` running 24/7

Estimated on `June 2023`

# TODO
- [ ] `This file is used by .ps1 utility scripts.` is not true yet. Implement this
- [ ] Add possibility of assigning some roles to users through terraform
    - [ ] Administrators
    - [ ] Developers with Secret Read permissions
    - [ ] Secrets maintainers
    - [ ] Infrastructure maintainers
- [ ] Add some sort of backup scripting of `.tfvars` file. It should not go to GIT (contains infra specific values and, 
potentially, secrets), but needs a way of sharing it with other devs.