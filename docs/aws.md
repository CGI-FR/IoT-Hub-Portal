# AWS configurations

## Overall Architecture

This schema represent the various components and how they interact to have a better understanding of the various solution elements.

![images/architecture_AWS.png](images/architecture_AWS.png)

1. The user is authenticated by the OpenID Connect server.
1. The user access to the IoT Hub Portal with the OAuth2.0 token.
1. The IoT Hub portal uses the AWS IoT REST API to retrieve the data.
1. The IoT Hub portal uses the AWS S3 storage to store the device models configuration (Images, Commands, etc.).
1. The IoT Hub portal synchronizes its data with the IoT Hub to provide a consistent view of the data.


## Configurations
This configurations are used to get access to AWS cloud Platform. You have to enter them in a `json` file to be able to connect to the Iot Hub Portal. Here is a template of a such `json` file.
```json
{
  "CloudProvider": "AWS",
  "S3Storage:ConnectionString": "<CONNECTION_STRING_S3_STORAGE>",
  "OIDC:Scope": "<SCOPE>",
  "OIDC:MetadataUrl": "<METADATA_URL>",
  "OIDC:ClientId": "<CLIENT_ID>",
  "OIDC:Authority": "<AUTHORITY>",
  "OIDC:ApiClientId": "<API_CLIENT_ID>",
  "PostgreSQL:ConnectionString": "<POSTGRE_SQL_CONNECTION_STRING>",
  "AWS:Access": "<AWS_ACCESS_KEY>",
  "AWS:AccessSecret": "<AWS_ACCESS_SECRET_KEY>",
  "AWS:Region": "<AWS_REGION_KEY>"
}
```
> <u>Note:</u> You must replace all values in the brackets by your own AWS settings. If you can't find them in the AWS Portal, please contact an administrator of this project to have more information.