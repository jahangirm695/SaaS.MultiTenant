FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SaaS.MultiTenant.sln", "./"]
COPY ["src/SaaS.Api/SaaS.Api.csproj", "src/SaaS.Api/"]
COPY ["src/SaaS.Application/SaaS.Application.csproj", "src/SaaS.Application/"]
COPY ["src/SaaS.Domain/SaaS.Domain.csproj", "src/SaaS.Domain/"]
COPY ["src/SaaS.Infrastructure/SaaS.Infrastructure.csproj", "src/SaaS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SaaS.MultiTenant.sln"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/SaaS.Api"
RUN dotnet build "SaaS.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SaaS.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SaaS.Api.dll"]
