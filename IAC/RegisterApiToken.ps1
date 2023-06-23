# Run for setup:
# az login
# az account list
# az account set --subscription "<subscription_Id>"

# Also requires to TEMPORARILY enable SSH connections. Do not leave enabled when not in use.

$resource_group_name = "rg-ek"
$key_vault_name = "kv-ek-discord"
$subscription = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).id
$tenant = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).tenantId

function ListSecrets {
    az keyvault secret list `
        --vault-name $key_vault_name | ConvertFrom-Json | Format-Table -Property name, tags, contentType
}

function SaveApiToken ([string] $name, [string] $secret) {
    # Set Discord Api Token
    az keyvault secret set `
        --name $name `
        --vault-name $key_vault_name `
        --value $secret
}

# Main token for discord bot authentication
SaveApiToken "Discord--Token"  "Your_discord_token" 

# Main token for notion authentication
SaveApiToken "Notion--Token"  "Your_notion_token" 
    
# Development Bot api token
SaveApiToken "DevDiscord--Token"  "Your_discord_token"
