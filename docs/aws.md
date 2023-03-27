# AWS configurations

## Overall Architecture

## Configurations
This configurations are used to get access to AWS cloud Platform. You have to enter them in a `json` file to be able to connect to the Iot Hub Portal. Here is a template of a such `json` file.
```json
{
  "CloudProvider": "AWS",
  "AWS:Access": "<AWS_ACCESS_KEY>",
  "AWS:AccessSecret": "<AWS_ACCESS_SECRET_KEY>",
  "AWS:Region": "<AWS_REGION_KEY>"
}
```
> <u>Note:</u> You must replace all values in the brackets by your own AWS settings. If you can't find them in the AWS Portal, please contact an administrator of this project to have more information.