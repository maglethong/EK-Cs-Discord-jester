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

# Generate Key vault
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

resource "azurerm_key_vault_secret" "kv-ek-dicord-token" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "Discord--Token"
  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
  value = "NOT SET"
}

resource "azurerm_key_vault_secret" "kv-ek-notion-token" {
  key_vault_id = azurerm_key_vault.vault.id
  name = "Notion--Token"
  # Never set you token here, since this will be pushed to REPO. Change it manually on azure or through AZ CLI
  value = "NOT SET"
}
