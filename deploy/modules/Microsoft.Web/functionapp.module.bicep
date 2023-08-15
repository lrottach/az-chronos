///////////////////////////////////////////////
//
//  Type:           Module
//  Author:         Lukas Rottach
//  CreationDate:   15.08.2023
//  Name:           Azure Function App
//  Provider:       Microsoft.Web
//
///////////////////////////////////////////////

////////////////////////////////
// Deployoment Scope
////////////////////////////////
targetScope = 'resourceGroup'

////////////////////////////////
// Parameter Area
////////////////////////////////

// Deployment parameter
param deploymentLocation string

// Function parameter
param functionAppName string

@description('The language worker runtime to load in the function app.')
@allowed([
  'dotnet'
  'dotnet-isolated'
])
param runtime string

@allowed([
  'v7.0'
  'v6.0'
])
param dotnetVersion string

// Storage parameter
param storageName string

////////////////////////////////
// Resource Area
////////////////////////////////

// Hosting Plan
resource asp 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: '${functionAppName}-asp1-we'
  location: deploymentLocation
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

// Storage Account
resource sto 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageName
  location: deploymentLocation
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

// Application Insights
resource appin 'Microsoft.Insights/components@2020-02-02' = {
  name: '${functionAppName}-appin1-we'
  location: deploymentLocation
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

// Azure Function App
resource func 'Microsoft.Web/sites@2022-09-01' = {
  name: functionAppName
  location: deploymentLocation
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: asp.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${sto.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${sto.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${sto.name};EndpointSuffix=${environment().suffixes.storage};AccountKey=${sto.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: appin.properties.InstrumentationKey
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: runtime
        }
      ]
      cors: {
        allowedOrigins: [
          'https://portal.azure.com'
        ]
      }
      netFrameworkVersion: dotnetVersion
      use32BitWorkerProcess: true
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}
