

///////////////////////////////////////////////
//
//  Type:           Main
//  Author:         Lukas Rottach
//  CreationDate:   15.08.2023
//  Name:           Azure Chronos Deployment
//
///////////////////////////////////////////////

////////////////////////////////
// Deployoment Scope
////////////////////////////////
targetScope = 'subscription'

////////////////////////////////
// Parameter Area
////////////////////////////////

// Deployment parameter
param deploymentLocation string
param rgName string

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
// Resource  Area
////////////////////////////////
resource rg 'Microsoft.Resources/resourceGroups@2022-09-01' = {
  name: rgName
  location: deploymentLocation
}

////////////////////////////////
// Module  Area
////////////////////////////////
module func './modules/Microsoft.Web/functionapp.module.bicep' = {
  scope: rg
  name: 'deploy-${rg.name}'
  params: {
    deploymentLocation: deploymentLocation
    dotnetVersion: dotnetVersion
    functionAppName: functionAppName
    runtime: runtime
    storageName: storageName
  }
}
