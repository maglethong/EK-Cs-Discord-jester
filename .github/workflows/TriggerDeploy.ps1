$storageAccountName = $env:RELEASE_STORAGE_ACCOUNT_NAME
$storageContainerName = $env:RELEASE_STORAGE_ACCOUNT_CONTAINER_NAME
$resource_group_name = $env:RESOURCE_GROUP_NAME
$vm_name = $env:DEPLOY_VM_NAME
$key_vault_name = $env:KEY_VAULT_NAME
$subscription = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).id
$tenant = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).tenantId
$deployStorageUrl="https://${storageAccountName}.blob.core.windows.net/${$storageContainerName}/"
$targetFileName = "./DockerImage-${{ github.sha }}.tar"

az vm run-command invoke `
          --resource-group $resource_group_name `
          --name $vm_name `
          --command-id "SetupVm" `
          --scripts "./.github/workflows/Deploy.sh >> Deploy.log" `
          --parameters $deployStorageUrl `
                       "${{ github.sha }}" `
                       "${{ secrets.AZURE_CREDENTIALS }}"