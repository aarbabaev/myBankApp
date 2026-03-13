FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["AA.SSO-service2/AA.SSO-service2.csproj", "AA.SSO-service2/"]
RUN dotnet restore "AA.SSO-service2/AA.SSO-service2.csproj"
COPY . .
WORKDIR "/src/AA.SSO-service2"
RUN dotnet build "./AA.SSO-service2.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AA.SSO-service2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AA.SSO-service2.dll"]
