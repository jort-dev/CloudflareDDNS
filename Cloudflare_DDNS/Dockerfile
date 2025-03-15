FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cloudflare_DDNS/Cloudflare_DDNS.csproj", "Cloudflare_DDNS/"]
RUN dotnet restore "Cloudflare_DDNS/Cloudflare_DDNS.csproj"
COPY . .
WORKDIR "/src/Cloudflare_DDNS"
RUN dotnet build "Cloudflare_DDNS.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Cloudflare_DDNS.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cloudflare_DDNS.dll"]
