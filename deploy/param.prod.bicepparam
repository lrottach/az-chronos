using 'main.bicep'

// Deployment Parameter
param deploymentLocation = 'West Europe'
param rgName = 'rg-prod-chronos1-we'

// Function Parameter
param functionAppName = 'azure-chronos'
param storageName = 'prodazchronossto1we'
param runtime = 'dotnet-isolated'
param dotnetVersion = 'v7.0'
