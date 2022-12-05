@description('Location for the resources.')
param location string

@description('App Insights name.')
param appInsighstName string

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  kind: 'web'
  name: appInsighstName
  location: location
  properties: {
    Application_Type: 'web'
  }
}

output id string = appInsights.id
