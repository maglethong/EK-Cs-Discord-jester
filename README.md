# EK-Cs-Discord-jester

Documentation for Discord Client: https://discordnet.dev/guides/getting_started/first-bot.html


# Run Locally

Download And Install `dotnet` SDK 7 at `https://download.visualstudio.microsoft.com/download/pr/974313ac-3d89-4c51-a6e8-338d864cf907/6ed5d4933878cada1b194dd1084a7e12/dotnet-sdk-7.0.302-win-x64.exe
`

run `dotnet dev-certs https --trust` to install development certificates

Set following environment variables:
``` bash
    # Required for starting discord bot. If not present, bot will not start.
    Discord__Token: #Discord Bot Token#
```