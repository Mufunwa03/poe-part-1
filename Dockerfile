# ============================================================
# Stage 1: Build the Azure Functions application
# ============================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy the complete application into the build container
COPY . .

# Restore NuGet packages
RUN dotnet restore *.csproj

# Build and publish the application
RUN dotnet publish *.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ============================================================
# Stage 2: Azure Functions runtime
# ============================================================

FROM mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0

WORKDIR /home/site/wwwroot

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Azure Functions configuration
ENV AzureWebJobsScriptRoot=/home/site/wwwroot
ENV AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Azure Functions listens on port 80 inside the container
EXPOSE 80