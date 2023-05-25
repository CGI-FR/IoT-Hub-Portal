// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using Amazon.IoT;
    using Amazon.IoT.Model;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Newtonsoft.Json.Linq;
    using Configuration = Microsoft.Azure.Devices.Configuration;

    public class AwsConfigService : IConfigService
    {
        private readonly IAmazonGreengrassV2 greengras;
        private readonly IAmazonIoT iotClient;

        public AwsConfigService(
            IAmazonGreengrassV2 greengras,
            IAmazonIoT iotClient)
        {
            this.greengras = greengras;
            this.iotClient = iotClient;
        }

        public async Task RollOutEdgeModelConfiguration(IoTEdgeModel edgeModel)
        {
            var createDeploymentRequest = new CreateDeploymentRequest
            {
                DeploymentName = edgeModel?.Name,
                Components = await CreateGreenGrassComponents(edgeModel!),
                TargetArn = await GetThingGroupArn(edgeModel!)
            };

            var createDeploymentResponse = await this.greengras.CreateDeploymentAsync(createDeploymentRequest);

            if (createDeploymentResponse.HttpStatusCode != HttpStatusCode.Created)
            {
                throw new InternalServerErrorException("The deployment creation failed due to an error in the Amazon IoT API.");

            }
        }

        private async Task<string> GetThingGroupArn(IoTEdgeModel edgeModel)
        {
            await CreateThingTypeIfNotExists(edgeModel!.Name);

            var dynamicThingGroup = new DescribeThingGroupRequest
            {
                ThingGroupName = edgeModel?.Name
            };

            try
            {
                var existingThingGroupResponse = await this.iotClient.DescribeThingGroupAsync(dynamicThingGroup);

                return existingThingGroupResponse.ThingGroupArn;
            }
            catch (Amazon.IoT.Model.ResourceNotFoundException)
            {
                var createThingGroupResponse = await this.iotClient.CreateDynamicThingGroupAsync(new CreateDynamicThingGroupRequest
                {
                    ThingGroupName = edgeModel!.Name,
                    QueryString = $"thingTypeName: {edgeModel!.Name}"
                });

                return createThingGroupResponse.ThingGroupArn;
            }
        }

        private async Task CreateThingTypeIfNotExists(string thingTypeName)
        {
            var existingThingType = new DescribeThingTypeRequest
            {
                ThingTypeName = thingTypeName
            };

            try
            {
                _ = await this.iotClient.DescribeThingTypeAsync(existingThingType);
            }
            catch (Amazon.IoT.Model.ResourceNotFoundException)
            {
                _ = await this.iotClient.CreateThingTypeAsync(new CreateThingTypeRequest
                {
                    ThingTypeName = thingTypeName,
                    Tags = new List<Tag>
                    {
                        new Tag
                        {
                            Key = "iotEdge",
                            Value = "True"
                        }
                    }
                });
            }
        }

        private async Task<Dictionary<string, ComponentDeploymentSpecification>> CreateGreenGrassComponents(IoTEdgeModel edgeModel)
        {
            var listcomponentName = new Dictionary<string, ComponentDeploymentSpecification>();
            foreach (var component in edgeModel.EdgeModules)
            {
                var recipeJson = JsonCreateComponent(component);
                var recipeBytes = Encoding.UTF8.GetBytes(recipeJson.ToString());
                var recipeStream = new MemoryStream(recipeBytes);

                var componentVersion = new CreateComponentVersionRequest
                {
                    InlineRecipe = recipeStream
                };
                var response = await greengras.CreateComponentVersionAsync(componentVersion);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.Created)
                {
                    throw new InternalServerErrorException("The component creation failed due to an error in the Amazon IoT API.");

                }
                listcomponentName.Add(component.ModuleName, new ComponentDeploymentSpecification { ComponentVersion = "1.0.0" });
            }

            return listcomponentName;
        }

        private static JObject JsonCreateComponent(IoTEdgeModule component)
        {
            var environmentVariableObject = new JObject();

            foreach (var env in component.EnvironmentVariables)
            {
                environmentVariableObject.Add(new JProperty(env.Name, env.Value));
            }

            var recipeJson =new JObject(
                    new JProperty("RecipeFormatVersion", "2020-01-25"),
                    new JProperty("ComponentName", component.ModuleName),
                    new JProperty("ComponentVersion", "1.0.0"),
                    new JProperty("ComponentPublisher", "IotHub"),
                    new JProperty("ComponentDependencies",
                        new JObject(
                            new JProperty("aws.greengrass.DockerApplicationManager",
                                new JObject(new JProperty("VersionRequirement", "~2.0.0"))),
                            new JProperty("aws.greengrass.TokenExchangeService",
                                new JObject(new JProperty("VersionRequirement", "~2.0.0")))
                        )
                    ),
                    new JProperty("Manifests",
                        new JArray(
                            new JObject(
                                new JProperty("Platform",
                                    new JObject(new JProperty("os", "linux"))),
                                new JProperty("Lifecycle",
                                    new JObject(new JProperty("Run", $"docker run {component.ImageURI}"),
                                                new JProperty("Environment",environmentVariableObject))),
                                    new JProperty("Artifacts",
                                        new JArray(
                                            new JObject(new JProperty("URI", $"docker:{component.ImageURI}"))
                                        )
                                    )
                            )
                        )
                    )
                );

            return recipeJson;
        }

        //AWS Not implemented methods

        public Task<IEnumerable<Configuration>> GetIoTEdgeConfigurations()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Configuration>> GetDevicesConfigurations()
        {
            throw new NotImplementedException();
        }

        public Task RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeviceModelConfigurationByConfigurationNamePrefix(string configurationNamePrefix)
        {
            throw new NotImplementedException();
        }

        public Task RollOutDeviceConfiguration(string modelId, Dictionary<string, object> desiredProperties, string configurationId, Dictionary<string, string> targetTags, int priority = 0)
        {
            throw new NotImplementedException();
        }

        public Task<Configuration> GetConfigItem(string id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteConfiguration(string configId)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetFailedDeploymentsCount()
        {
            throw new NotImplementedException();
        }

        public Task<List<IoTEdgeModule>> GetConfigModuleList(string modelId)
        {
            // To be implemented with the update method in EdgeModelService
            throw new NotImplementedException();
        }

        public Task<List<EdgeModelSystemModule>> GetModelSystemModule(string modelId)
        {
            throw new NotImplementedException();
        }

        public Task<List<IoTEdgeRoute>> GetConfigRouteList(string modelId)
        {
            throw new NotImplementedException();
        }

    }
}
