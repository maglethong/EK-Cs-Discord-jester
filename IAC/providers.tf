
terraform {
  required_version = ">=1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3.0"
    }
    tls = {
      source = "hashicorp/tls"
      version = "~>4.0"
    }
    local = {
      source = "hashicorp/local"
      version = "~>1.0"
    }
  }
  # Comment out on first run so the storage account gets created
  # Also make sure resource_group_name and storage_account_name match the created values. (Variables are not allowed here)
  backend "azurerm" {
    resource_group_name  = "rg-ek"
    storage_account_name = "dtekdiscterraformsa"
    container_name       = "terraformstate"
    key                  = "terraform.tfstate"
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = true
    }
  }
}
