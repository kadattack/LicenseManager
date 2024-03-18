FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LicenseAuthenticationServer/LicenseAuthenticationServer.csproj", "LicenseAuthenticationServer/"]
RUN dotnet restore "LicenseAuthenticationServer/LicenseAuthenticationServer.csproj"
COPY . .
WORKDIR "/src/LicenseAuthenticationServer"
RUN dotnet build "LicenseAuthenticationServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LicenseAuthenticationServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LicenseAuthenticationServer.dll"]
