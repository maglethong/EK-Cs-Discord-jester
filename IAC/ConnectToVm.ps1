# Run for setup:
# az login
# az account list
# az account set --subscription "<subscription_Id>"

# Also requires to TEMPORARILY enable SSH connections. Do not leave enabled when not in use.

$resource_group_name = "rg-ek"
$key_vault_name = "kv-ek-discord"
$vm_name = "mv-myvm"
$vm_admin_user = "azureuser"
$ssh_private_key_secret_name = "kv-ek-my-vm-ssh-priv"
$vm_ip_secret_name = "kv-ek-my-vm-ip"

$subscription = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).id
$tenant = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).tenantId

# Connect to VM via ssh
$response = az keyvault secret show `
    --name $ssh_private_key_secret_name `
    --vault-name $key_vault_name
    
$priv_key = ($response | ConvertFrom-Json).Value

$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
[System.IO.File]::WriteAllLines("$env:USERPROFILE/.ssh/$ssh_private_key_secret_name", $priv_key, $Utf8NoBomEncoding)

$response = az keyvault secret show `
    --name $vm_ip_secret_name `
    --vault-name $key_vault_name
    
$ip =  ($response | ConvertFrom-Json).Value
    
ssh -i "$env:USERPROFILE/.ssh/$ssh_private_key_secret_name" "$vm_admin_user@$ip"

$sp = (((az keyvault secret show  `
  --vault-name $key_vault_name `
  --name "EK-Discord-Jester--ServicePrincipal--credentials") | `
  ConvertFrom-Json).value | `
  ConvertFrom-Json)
  
 az logout
 
 az login `
   --service-principal `
   --username $sp.clientId `
   -p $sp.clientSecret `
   --tenant $sp.tenantId
   
   

