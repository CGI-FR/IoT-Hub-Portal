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
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10;
    using Newtonsoft.Json.Linq;
    using Configuration = Microsoft.Azure.Devices.Configuration;

    public class AwsConfigService : IConfigService
    {
        private readonly IAmazonGreengrassV2 greengras;
        private readonly IAmazonIoT iotClient;

        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceModelRepository edgeModelRepository;
        private readonly ConfigHandler config;
        private readonly IMapper mapper;

        public AwsConfigService(
            IAmazonGreengrassV2 greengras,
            IAmazonIoT iot,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceModelRepository edgeModelRepository,
            ConfigHandler config)
        {
            this.greengras = greengras;
            this.iotClient = iot;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeModelRepository = edgeModelRepository;
            this.config = config;
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
            else
            {
                var edgeModelEntity = await this.edgeModelRepository.GetByIdAsync(edgeModel?.ModelId!);
                if (edgeModelEntity == null)
                {
                    throw new Domain.Exceptions.ResourceNotFoundException($"The edge model with id {edgeModel?.ModelId} not found");

                }
                else
                {
                    edgeModel!.IdProvider = createDeploymentResponse.DeploymentId;

                    _ = this.mapper.Map(edgeModel, edgeModelEntity);

                    this.edgeModelRepository.Update(edgeModelEntity);
                    await this.unitOfWork.SaveAsync();
                }

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

                if (response.HttpStatusCode != HttpStatusCode.Created)
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

        public async Task<List<IoTEdgeModule>> GetConfigModuleList(string modelId)
        {

            var moduleList = new List<IoTEdgeModule>();

            var getDeployement = new GetDeploymentRequest
            {
                DeploymentId = modelId,
            };
            try
            {
                var response = await this.greengras.GetDeploymentAsync(getDeployement);

                foreach (var compoenent in response.Components)
                {
                    var responseComponent = await this.greengras.GetComponentAsync(new GetComponentRequest
                    {
                        Arn = $"arn:aws:greengrass:{config.AWSRegion}:{config.AWSAccountId}:components:{compoenent.Key}:versions:{compoenent.Value.ComponentVersion}",
                        RecipeOutputFormat = RecipeOutputFormat.JSON
                    });

                    // Read the Recipe which is in JSON Format
                    using var reader = new StreamReader(responseComponent.Recipe);
                    var recipeJsonString = reader.ReadToEnd();

                    // Extract the imageUri from the 'Run' JSON object
                    var uriImage = retreiveImageUri("Lifecycle", "Run", recipeJsonString);
                    // Extract the environment Variables from the 'Environment' JSON object
                    var env = retreiveEnvVariableAttr("Lifecycle", "Environment", recipeJsonString);

                    var iotEdgeModule = new IoTEdgeModule
                    {
                        ModuleName = compoenent.Key,
                        ImageURI = uriImage,
                        EnvironmentVariables = env
                    };

                    moduleList.Add(iotEdgeModule);

                }
                return moduleList;
            }
            catch (Amazon.IoT.Model.ResourceNotFoundException)
            {
                throw new InternalServerErrorException("The deployment is not found");

            }
        }

        private static string retreiveImageUri(string parent, string child, string recipeJsonString)
        {
            var uriImage = "";
            // Parse the string as a JSON object
            var recipeJsonObject = JObject.Parse(recipeJsonString);

            // Extract the "Manifests" array
            var jArray = recipeJsonObject["Manifests"] as JArray;
            var manifests = jArray;

            if (manifests != null && manifests.Count > 0)
            {
                // Get the first manifest in the array
                var firstManifest = manifests[0] as JObject;

                // Extract the "Lifecycle" object
                var jObject = firstManifest?[parent] as JObject;
                var lifecycle = jObject;

                if (lifecycle != null)
                {
                    // Extract the value of "Run"
                    var runValue = lifecycle[child]?.ToString();

                    // Search the index of the 1st whitespace
                    var firstSpaceIndex = runValue.IndexOf(' ');

                    if (firstSpaceIndex != -1)
                    {
                        // // Search the index of the 2nd whitespace
                        var secondSpaceIndex = runValue.IndexOf(' ', firstSpaceIndex + 1);

                        if (secondSpaceIndex != -1)
                        {
                            // Extract the URI iamge
                            uriImage = runValue[(secondSpaceIndex + 1)..];
                        }

                    }
                }
            }

            return uriImage;
        }

        private static List<IoTEdgeModuleEnvironmentVariable> retreiveEnvVariableAttr(string parent, string child, string recipeJsonString)
        {

            // Parse the string as a JSON object
            var recipeJsonObject = JObject.Parse(recipeJsonString);

            var environmentVariables = new List<IoTEdgeModuleEnvironmentVariable>();

            // Extract the "Manifests" array
            var jArray = recipeJsonObject["Manifests"] as JArray;
            var manifests = jArray;

            if (manifests != null && manifests.Count > 0)
            {
                // Get the first manifest in the array
                var firstManifest = manifests[0] as JObject;

                // Extract the "Lifecycle" object
                var jObject = firstManifest?[parent] as JObject;
                var lifecycle = jObject;

                if (lifecycle != null)
                {
                    // Extract the value of "Environment"
                    var env = lifecycle?[child] as JObject;

                    // Convert Environment JSON Object as a dictionnary
                    var keyValuePairs = env!.ToObject<Dictionary<string, string>>();

                    foreach (var kvp in keyValuePairs!)
                    {
                        var iotEnvVariable = new IoTEdgeModuleEnvironmentVariable
                        {
                            Name = kvp.Key,
                            Value = kvp.Value
                        };

                        environmentVariables.Add(iotEnvVariable);
                    }
                }
            }
            return environmentVariables;
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
