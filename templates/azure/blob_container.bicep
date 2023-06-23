@description('Storage Account name.')
param storageAccountName string

@description('Storage Account name.')
param deviceImageContainerName string

resource storageAccountName_default_deviceImageContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-05-01' = {
  name: '${storageAccountName}/default/${deviceImageContainerName}'
}
