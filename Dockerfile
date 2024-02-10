FROM mcr.microsoft.com/dotnet/runtime:8.0-cbl-mariner-distroless AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-cbl-mariner AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
WORKDIR "/src/FakeA2SServer"
RUN dotnet build "FakeA2SServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FakeA2SServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FakeA2SServer.dll"]
