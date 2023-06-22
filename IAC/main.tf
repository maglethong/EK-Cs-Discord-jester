# Generate Resource group
resource "azurerm_resource_group" "rg" {
  name     = var.resource_group_name
  location = var.resource_group_location
}

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

resource "azurerm_storage_container" "terraform_storage_container" {
  name                  = "terraformstate"
  storage_account_name  = azurerm_storage_account.terraform_storage_account.name
  container_access_type = "private"
}

# Generate Key vault
# ~0.1$ / Month on June 2023
resource "random_string" "azurerm_key_vault_name" {
  length  = 13
  lower   = true
  numeric = false
  special = false
  upper   = false
}

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

#resource "azurerm_key_vault_secret" "kv-ek-dicord-token" {
#  key_vault_id = azurerm_key_vault.vault.id
#  name = "Discord--Token"
#  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
#  value = "NOT SET"
#}
#
#resource "azurerm_key_vault_secret" "kv-ek-notion-token" {
#  key_vault_id = azurerm_key_vault.vault.id
#  name = "Notion--Token"
#  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
#  value = "NOT SET"
#}

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

  # TODO: allowing SSH is dangerous. We should block this in the future
#  security_rule {
#    name                       = "SSH"
#    priority                   = 1001
#    direction                  = "Inbound"
#    access                     = "Allow"
#    protocol                   = "Tcp"
#    source_port_range          = "*"
#    destination_port_range     = "22"
#    source_address_prefix      = "*"
#    destination_address_prefix = "*"
#  }
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
  name                  = "mv-myvm"
  location              = azurerm_resource_group.rg.location
  resource_group_name   = azurerm_resource_group.rg.name
  network_interface_ids = [azurerm_network_interface.my_terraform_nic.id]
  # ~3.8$ / Month on June 2023
  size                  = "Standard_B1ls"
  # ~7.6$ / Month on June 2023
#  size                  = "Standard_B1s"

  # ~2.4$ / Month on June 2023
  os_disk {
    name                 = "md-myosdisk"
    caching              = "ReadWrite"
    storage_account_type = "Standard_LRS"
    disk_size_gb         = "30"
  }

  source_image_reference {
    publisher = "Canonical"
    offer     = "0001-com-ubuntu-server-jammy"
    sku       = "22_04-lts-gen2"
    version   = "latest"
  }

  computer_name                   = "myvm"
  admin_username                  = "azureuser"
#  admin_password                  = "Admin123password"
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
  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
  value = tls_private_key.example_ssh.public_key_openssh
}

resource "azurerm_key_vault_secret" "kv-ek-myvm-ssh-priv" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "kv-ek-my-vm-ssh-priv"
  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
  value = tls_private_key.example_ssh.private_key_openssh
}

# Total estimated price:
# ~6.6$ / Month Total