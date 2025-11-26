FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["leafy-transport.api/leafy-transport.api.csproj", "leafy-transport.api/"]
COPY ["leafy-transport.models/leafy-transport.models.csproj", "leafy-transport.models/"]
RUN dotnet restore "leafy-transport.api/leafy-transport.api.csproj"
COPY . .
WORKDIR "/src/leafy-transport.api"
RUN dotnet build "leafy-transport.api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "leafy-transport.api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "leafy-transport.api.dll"]
