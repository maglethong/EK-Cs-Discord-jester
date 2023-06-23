# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# copy csproj and restore as distinct layers
WORKDIR /source
COPY . .
#COPY *.sln .
#COPY ./EK.Discord.Bot/*.csproj ./EK.Discord.Bot/
#COPY ./EK.Discord.Client/*.csproj ./EK.Discord.Client/
#COPY ./EK.Discord.Common/*.csproj ./EK.Discord.Common/
#COPY ./EK.Discord.Server/*.csproj ./EK.Discord.Server/
#COPY ./*.props ./
RUN dotnet restore ./*.sln
RUN dotnet workload restore

# TODO Somehow this keeeps crashing... needs to be optimised so we make use of NuGet cache
# copy everything else and build app
#COPY ./EK.Discord.Bot/* ./EK.Discord.Bot/
#COPY ./EK.Discord.Client/* ./EK.Discord.Client/
#COPY ./EK.Discord.Common/* ./EK.Discord.Common/
#COPY ./EK.Discord.Server/* ./EK.Discord.Server/
WORKDIR /source/build
RUN dotnet publish \
    /source/EK.Discord.Server/EK.Discord.Server.csproj \
    --configuration Release \
    --output /Build/Linux/ \
    --self-contained \
    --runtime linux-x64 \
    --no-restore

# final stage/image
WORKDIR /Build/Linux
FROM mcr.microsoft.com/dotnet/aspnet:7.0
COPY --from=build /Build/Linux ./
ENTRYPOINT ["dotnet", "EK.Discord.Server.dll"]
