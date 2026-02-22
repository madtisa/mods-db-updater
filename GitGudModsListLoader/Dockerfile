FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["GitGudModsListLoader/GitGudModsListLoader.csproj", "GitGudModsListLoader/"]
RUN dotnet restore "./GitGudModsListLoader/GitGudModsListLoader.csproj"
COPY . .
WORKDIR "/src/GitGudModsListLoader"
RUN dotnet build "./GitGudModsListLoader.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./GitGudModsListLoader.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitGudModsListLoader.dll"]