$imageFile = "./DockerImage.tar"
$storageAccountName = $env:RELEASE_STORAGE_ACCOUNT_NAME
$storageContainerName = $env:RELEASE_STORAGE_ACCOUNT_CONTAINER_NAME
$targetFileName = "./DockerImage-${{ github.sha }}.tar"
$subscription = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).id
$tenant = ((az account list | ConvertFrom-Json) | WHERE isDefault -eq $true).tenantId

Write-Host Uploading release to $targetFileName

az storage blob upload `
    --file $imageFile `
    --name $targetFileName `
    --account-name $storageAccountName `
    --container-name $storageContainerName `
    --auth-mode login `
    --timeout 300

$existingReleasesRaw = az storage blob list `
    --account-name $storageAccountName `
    --container-name $storageContainerName `
    --auth-mode login `
    --timeout 300

$toDeleteList = ($existingReleasesRaw | `
                 ConvertFrom-Json | `
                 Sort-Object $_.properties.lastModified) | `
    Select-Object `
    -Skip 3 `
    -Property name

foreach ($fileToDelete in $toDeleteList) {
    Write-Host Deleting old release file: $fileToDelete.Name
    az storage blob delete `
        --name $fileToDelete.Name `
        --account-name $storageAccountName `
        --container-name $storageContainerName `
        --auth-mode login `
        --timeout 300
}