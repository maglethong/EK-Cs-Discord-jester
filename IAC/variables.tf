# For full list see https://github.com/claranet/terraform-azurerm-regions/blob/master/REGIONS.md

variable "host_vm" {
  type = object({
    enable_ssh = bool
    size       = string
  })
  description  = "Configuration variables for the vm hosting the application"
  default      = ({
    enable_ssh = false
    # ~3.8$ / Month on June 2023
    size       = "Standard_B1ls"
    # ~7.6$ / Month on June 2023
    #size       = "Standard_B1s"
  })
}

variable "ssh_private_key_secret_name" {
  type        = string
  description = "Name of the secret storing the host Vm's private key for ssh access"
  default     = "kv-ek-my-vm-ssh-priv"
}

variable "vm_ip_secret_name" {
  type        = string
  description = "Name of the secret storing the host Vm's IP address"
  default     = "kv-ek-my-vm-ip"
}

variable "host_vm_admin_user" {
  type        = string
  description = "Name of the vm admin username"
  default     = "azureuser"
}
        
variable "host_vm_name" {
  type        = string
  description = "Name of the vm hosting the application"
  default     = "mv-myvm"
}

variable "resource_group_location" {
  type        = string
  description = "Location for all resources."
  default     = "eastus"
}

variable "resource_group_name" {
  type        = string
  description = "Prefix of the resource group name that's combined with a random ID so name is unique in your Azure subscription."
  default     = "rg-ek"
}

variable "vault_name" {
  type        = string
  description = "The name of the key vault to be created."
  default     = "kv-ek-discord"
}

variable "msi_id" {
  type        = string
  description = "The Managed Service Identity ID. If this value isn't null (the default), 'data.azurerm_client_config.current.object_id' will be set to this value."
  default     = null
}

variable "service_principal_expiration_date" {
  type        = string
  description = "The xpiration date of the service principal. After this date, the SP neets to be re-generated and it's secret re-inserted into the pipeline."
  default     = "2024-01-01T00:00:00Z"
}