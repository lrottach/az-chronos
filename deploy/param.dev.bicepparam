using 'main.bicep'

// Deployment Parameter
param deploymentLocation = 'West Europe'
param rgName = 'rg-dev-chronos1-we'

// Function Parameter
param functionAppName = 'azure-chronos-dev'
param storageName = 'devazchronossto1we'
param runtime = 'dotnet-isolated'
param dotnetVersion = 'v7.0'
