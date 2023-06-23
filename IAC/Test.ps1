# az login
# az account list
# az account set --subscription "<subscription_Id>"

$resource_group_name = "rg-ek"
$key_vault_name = "kv-ek-discord"
$vm_name = "mv-myvm"
$vm_admin_user = "azureuser"
$ssh_private_key_secret_name = "kv-ek-my-vm-ssh-priv"
$vm_ip_secret_name = "kv-ek-my-vm-ip"


az vm run-command invoke `
    --resource-group $resource_group_name `
    --name $vm_name `
    --command-id "RunShellScript" `
    --scripts "echo $1 $2" `
#     --scripts @script.ps1 `
    --parameters "hello" "world"
    
    
az vm run-command invoke `
    --resource-group $resource_group_name `
    --name $vm_name `
    --command-id "RunShellScript" `
    --scripts "echo $1 $2" `
    --parameters "hello" "world"
