# Generate Resource group
resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.resource_group_location
}

# Identify current subscription
data "azurerm_subscription" "primary" {}

# Identify current user
data "azurerm_client_config" "current" {}

locals {
  current_user_id = coalesce(var.msi_id, data.azurerm_client_config.current.object_id)
}

# Create storage account for storing terraform state
resource "azurerm_storage_account" "terraform_storage_account" {
  name                     = "dtekdiscterraformsa"
  location                 = azurerm_resource_group.rg.location
  resource_group_name      = azurerm_resource_group.rg.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Terraform state container
resource "azurerm_storage_container" "terraform_storage_container" {
  name                  = "terraformstate"
  storage_account_name  = azurerm_storage_account.terraform_storage_account.name
  container_access_type = "private"
}

# Generate Key vault
# ~0.1$ / Month on June 2023
resource "azurerm_key_vault" "vault" {
  name                       = var.vault_name
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = var.sku_name
  soft_delete_retention_days = 7

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = local.current_user_id

    key_permissions    = var.key_permissions
    secret_permissions = var.secret_permissions
  }
}

resource "azurerm_key_vault" "dev_vault" {
  name                       = "dev-${var.vault_name}"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = var.sku_name
  soft_delete_retention_days = 7

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = local.current_user_id

    key_permissions    = var.key_permissions
    secret_permissions = var.secret_permissions
  }
}

#####################################
# Create VM to Host our Application #
#####################################
# see https://learn.microsoft.com/en-us/azure/virtual-machines/linux/quick-create-terraform

# Create virtual network
resource "azurerm_virtual_network" "my_terraform_network" {
  name                = "vn-myvnet"
  address_space       = ["10.0.0.0/16"]
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
}

# Create subnet
resource "azurerm_subnet" "my_terraform_subnet" {
  name                 = "nt-mysubnet"
  resource_group_name  = azurerm_resource_group.rg.name
  virtual_network_name = azurerm_virtual_network.my_terraform_network.name
  address_prefixes     = ["10.0.1.0/24"]
}

# Create public IPs
resource "azurerm_public_ip" "my_terraform_public_ip" {
  name                = "ip-mypublicip"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  allocation_method   = "Dynamic"
}

# Create Network Security Group and rule
resource "azurerm_network_security_group" "my_terraform_nsg" {
  name                = "sg-myNetworkSecurityGroup"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  # Only temporarily enable when in use
  security_rule {
    name                       = "SSH"
    priority                   = 1001
    direction                  = "Inbound"
    access                     = var.host_vm.enable_ssh == true ? "Allow" : "Deny"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "22"
    source_address_prefix      = "*"
    destination_address_prefix = "*"
  }
}

# Create network interface
resource "azurerm_network_interface" "my_terraform_nic" {
  name                = "nic-mynic"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name

  ip_configuration {
    name                          = "my_nic_configuration"
    subnet_id                     = azurerm_subnet.my_terraform_subnet.id
    private_ip_address_allocation = "Dynamic"
    public_ip_address_id          = azurerm_public_ip.my_terraform_public_ip.id
  }
}

# Connect the security group to the network interface
resource "azurerm_network_interface_security_group_association" "example" {
  network_interface_id      = azurerm_network_interface.my_terraform_nic.id
  network_security_group_id = azurerm_network_security_group.my_terraform_nsg.id
}

# Create storage account for boot diagnostics
# see https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs/data-sources/storage_account
# ~0.3$ / Month on June 2023
resource "azurerm_storage_account" "my_storage_account" {
  name                     = "stdiagmyvm"
  location                 = azurerm_resource_group.rg.location
  resource_group_name      = azurerm_resource_group.rg.name
  account_tier             = "Standard"
  account_replication_type = "LRS"
}

# Create (and display) an SSH key
resource "tls_private_key" "example_ssh" {
  algorithm = "RSA"
  rsa_bits  = 4096
}

# Create virtual machine
resource "azurerm_linux_virtual_machine" "my_terraform_vm" {
  
  count = var.host_vm.create == true ? 1 : 0

  name                  = var.host_vm.name
  location              = azurerm_resource_group.rg.location
  resource_group_name   = azurerm_resource_group.rg.name
  network_interface_ids = [azurerm_network_interface.my_terraform_nic.id]
  size                  = var.host_vm.size

  # ~2.4$ / Month on June 2023
  os_disk {
    name                 = "md-myosdisk"
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
    disk_size_gb         = "30"
  }

  # See: https://learn.microsoft.com/en-us/azure/virtual-machines/linux/cli-ps-findimage
  source_image_reference {
    publisher = "Debian"
    offer     = "debian-10"
    sku       = "10"
    version   = "latest"
  }

  computer_name                   = "myvm"
  admin_username                  = "azureuser"
  disable_password_authentication = true

  admin_ssh_key {
    username   = "azureuser"
    public_key = tls_private_key.example_ssh.public_key_openssh
  }

  boot_diagnostics {
    storage_account_uri = azurerm_storage_account.my_storage_account.primary_blob_endpoint
  }
}

resource "azurerm_key_vault_secret" "kv-ek-myvm-ssh-pub" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "kv-ek-my-vm-ssh-pub"
  content_type = "SSH Public Key"
  value = tls_private_key.example_ssh.public_key_openssh
}

resource "azurerm_key_vault_secret" "kv-ek-myvm-ssh-priv" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "kv-ek-my-vm-ssh-priv"
  content_type = "SSH Private Key"
  value = tls_private_key.example_ssh.private_key_openssh
}

resource "azurerm_key_vault_secret" "kv-ek-myvm-ssh-pem" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "kv-ek-my-vm-ssh-pem"
  content_type = "SSH .PEM"
  value = tls_private_key.example_ssh.private_key_pem
}

resource "azurerm_key_vault_secret" "kv-ek-myvm-ip" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "kv-ek-my-vm-ip"
  content_type = "IP"
  value = azurerm_public_ip.my_terraform_public_ip.ip_address
}

# Create service Principal
resource "azuread_application" "Application" {
  display_name = "EK-Discord-Jester"
}

resource "azuread_service_principal" "Application_Pipeline" {
  application_id       = azuread_application.Application.application_id
}

resource "azuread_service_principal_password" "Application_Pipeline" {
  service_principal_id = "${azuread_service_principal.Application_Pipeline.id}"
  end_date_relative    = "240h"
}

# Pipeline Releases container
resource "azurerm_storage_container" "releases_storage_container" {
  name                  = "publicreleases"
  storage_account_name  = azurerm_storage_account.terraform_storage_account.name
  container_access_type = "container"
}

resource "azurerm_role_assignment" "Admin_StorageAccount" {
  scope                = data.azurerm_subscription.primary.id
  role_definition_name = "Storage Account Contributor"
  principal_id         = local.current_user_id
}

resource "azurerm_role_assignment" "Admin_Blobs" {
  scope                = data.azurerm_subscription.primary.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = local.current_user_id
}

resource "azurerm_role_assignment" "Application_Pipeline_releases_StorageAccount" {
  scope                = azurerm_storage_container.releases_storage_container.resource_manager_id
  role_definition_name = "Storage Account Contributor"
  principal_id         = azuread_service_principal.Application_Pipeline.id
}

resource "azurerm_role_assignment" "Application_Pipeline_releases_Blobs" {
  scope                = azurerm_storage_container.releases_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azuread_service_principal.Application_Pipeline.id
}

// TODO create custom role with only necessary permissions for pipeline
resource "azurerm_role_assignment" "Application_Pipeline" {
  scope                = azurerm_key_vault.vault.id
  role_definition_name = "Reader"
  principal_id         = azuread_service_principal.Application_Pipeline.id
}

resource "azurerm_key_vault_secret" "Application_Pipeline" {
  key_vault_id = azurerm_key_vault.vault.id
  content_type = "JSON"
  name = "EK-Discord-Jester--ServicePrincipal--credentials"
  value = "{\"clientId\":\"${azuread_service_principal.Application_Pipeline.application_id}\",\"clientSecret\":\"${azuread_service_principal_password.Application_Pipeline.value}\",\"subscriptionId\":\"${data.azurerm_subscription.primary.id}\",\"tenantId\":\"${data.azurerm_subscription.primary.tenant_id}\"}"
}

# Total estimated price:
# ~6.6$ / Month Total