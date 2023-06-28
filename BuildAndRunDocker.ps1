docker build -t maglethong/ek/discord/jester:latest .

az login

$key_vault_name = "dev-kv-ek-discord"

$Discord__Token = (az keyvault secret show `
    --name "Discord--Token" `
    --vault-name $key_vault_name | ConvertFrom-Json).Value

$Notion__Token = (az keyvault secret show `
    --name "Notion--Token" `
    --vault-name $key_vault_name | ConvertFrom-Json).Value

docker run `
    --rm `
    -d `
    --name ek-discord-jester `
    -p 9316:9316 `
    -p 5025:5025 `
    -p 7144:7144 `
    -p 44376:44376 `
    -e Discord__Token=$Discord__Token `
    -e Notion__Token=$Notion__Token `
    -e ASPNETCORE_ENVIRONMENT='Development' `
    -e DOTNET_ENVIRONMENT='Development' `
    maglethong/ek/discord/jester:latest