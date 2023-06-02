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
    using Configuration = Microsoft.Azure.Devices.Configuration;

    public class AwsConfigService : IConfigService
    {
        private readonly IAmazonGreengrassV2 greengrass;
        private readonly IAmazonIoT iotClient;

        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceModelRepository edgeModelRepository;
        private readonly ConfigHandler config;
        private readonly IMapper mapper;

        public AwsConfigService(
            IAmazonGreengrassV2 greengrass,
            IAmazonIoT iot,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceModelRepository edgeModelRepository,
            ConfigHandler config)
        {
            this.greengrass = greengrass;
            this.iotClient = iot;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeModelRepository = edgeModelRepository;
            this.config = config;
        }

        public async Task<string> RollOutEdgeModelConfiguration(IoTEdgeModel edgeModel)
        {

            var createDeploymentRequest = new CreateDeploymentRequest
            {
                DeploymentName = edgeModel?.Name,
                Components = await CreateGreenGrassComponents(edgeModel!),
                TargetArn = await GetThingGroupArn(edgeModel!)
            };

            var createDeploymentResponse = await this.greengrass.CreateDeploymentAsync(createDeploymentRequest);

            if (createDeploymentResponse.HttpStatusCode != HttpStatusCode.Created)
            {
                throw new InternalServerErrorException("The deployment creation failed due to an error in the Amazon IoT API.");
            }

            return createDeploymentResponse.DeploymentId;
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
            var components = new Dictionary<string, ComponentDeploymentSpecification>();
            foreach (var component in edgeModel.EdgeModules)
            {
                try
                {
                    var componentArn = !string.IsNullOrEmpty(component.Id) ?
                        $"{component.Id}:versions:{component.Version}" : // Public greengrass component
                        $"arn:aws:greengrass:{config.AWSRegion}:{config.AWSAccountId}:components:{component.ModuleName}:versions:{component.Version}"; //

                    _ = await this.greengrass.DescribeComponentAsync(new DescribeComponentRequest
                    {
                        Arn = componentArn
                    });
                    components.Add(component.ModuleName, new ComponentDeploymentSpecification { ComponentVersion = component.Version });

                }
                catch (Amazon.GreengrassV2.Model.ResourceNotFoundException)
                {
                    var componentVersion = new CreateComponentVersionRequest
                    {
                        InlineRecipe = new MemoryStream(Encoding.UTF8.GetBytes(component.ContainerCreateOptions))
                    };
                    var response = await greengrass.CreateComponentVersionAsync(componentVersion);
                    if (response.HttpStatusCode != HttpStatusCode.Created)
                    {
                        throw new InternalServerErrorException("The component creation failed due to an error in the Amazon IoT API.");

                    }
                    components.Add(component.ModuleName, new ComponentDeploymentSpecification { ComponentVersion = component.Version });
                }
            }

            return components;
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

        public Task<string> RollOutDeviceModelConfiguration(string modelId, Dictionary<string, object> desiredProperties)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDeviceModelConfigurationByConfigurationNamePrefix(string configurationNamePrefix)
        {
            throw new NotImplementedException();
        }

        public Task<string> RollOutDeviceConfiguration(string modelId, Dictionary<string, object> desiredProperties, string configurationId, Dictionary<string, string> targetTags, int priority = 0)
        {
            throw new NotImplementedException();
        }

        public Task<Configuration> GetConfigItem(string id)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteConfiguration(string modelId)
        {
            var modules = await GetConfigModuleList(modelId);

            foreach (var module in modules.Where(c => string.IsNullOrEmpty(c.Id)))
            {
                var deletedComponentResponse = await this.greengrass.DeleteComponentAsync(new DeleteComponentRequest
                {
                    Arn = $"arn:aws:greengrass:{config.AWSRegion}:{config.AWSAccountId}:components:{module.ModuleName}:versions:{module.Version}"
                });

                if (deletedComponentResponse.HttpStatusCode != HttpStatusCode.NoContent)
                {
                    throw new InternalServerErrorException("The deletion of the component failed due to an error in the Amazon IoT API.");
                }
            }

            var cancelDeploymentResponse = await this.greengrass.CancelDeploymentAsync(new CancelDeploymentRequest
            {
                DeploymentId = modelId
            });

            if (cancelDeploymentResponse.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new InternalServerErrorException("The cancellation of the deployment failed due to an error in the Amazon IoT API.");

            }
            else
            {
                var deleteDeploymentResponse = await this.greengrass.DeleteDeploymentAsync(new DeleteDeploymentRequest
                {
                    DeploymentId = modelId
                });

                if (deleteDeploymentResponse.HttpStatusCode != HttpStatusCode.NoContent)
                {
                    throw new InternalServerErrorException("The deletion of the deployment failed due to an error in the Amazon IoT API.");
                }
            }
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
                var response = await this.greengrass.GetDeploymentAsync(getDeployement);

                foreach (var compoenent in response.Components)
                {
                    var componentId = string.Empty;
                    var jsonRecipe = string.Empty;

                    try
                    {
                        var responseComponent = await this.greengrass.GetComponentAsync(new GetComponentRequest
                        {
                            Arn = $"arn:aws:greengrass:{config.AWSRegion}:{config.AWSAccountId}:components:{compoenent.Key}:versions:{compoenent.Value.ComponentVersion}",
                            RecipeOutputFormat = RecipeOutputFormat.JSON
                        });

                        // Read the Recipe which is in JSON Format
                        using var reader = new StreamReader(responseComponent.Recipe);
                        jsonRecipe = reader.ReadToEnd();
                    }
                    catch (Amazon.GreengrassV2.Model.ResourceNotFoundException)
                    {
                        // If the component is not found, we assume it is a public component
                        componentId = $"arn:aws:greengrass:{config.AWSRegion}:aws:components:{compoenent.Key}";

                        var responseComponent = await this.greengrass.GetComponentAsync(new GetComponentRequest
                        {
                            Arn = $"arn:aws:greengrass:{config.AWSRegion}:aws:components:{compoenent.Key}:versions:{compoenent.Value.ComponentVersion}",
                            RecipeOutputFormat = RecipeOutputFormat.JSON
                        });

                        using var reader = new StreamReader(responseComponent.Recipe);
                        jsonRecipe = reader.ReadToEnd();
                    }

                    var iotEdgeModule = new IoTEdgeModule
                    {
                        Id = componentId,
                        ModuleName = compoenent.Key,
                        Version = compoenent.Value.ComponentVersion,
                        ContainerCreateOptions = jsonRecipe,
                        // ImageURI is required, but not used for Greengrass components
                        ImageURI = "example.com"
                    };

                    moduleList.Add(iotEdgeModule);

                }
                return moduleList;
            }
            catch (Amazon.GreengrassV2.Model.ResourceNotFoundException)
            {
                throw new InternalServerErrorException("The deployment is not found");

            }
        }

        public Task<List<EdgeModelSystemModule>> GetModelSystemModule(string modelId)
        {
            throw new NotImplementedException();
        }

        public Task<List<IoTEdgeRoute>> GetConfigRouteList(string modelId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IoTEdgeModule>> GetPublicEdgeModules()
        {
            var publicComponents = new List<Component>();

            var nextToken = string.Empty;

            do
            {
                var response = await this.greengrass.ListComponentsAsync(new ListComponentsRequest
                {
                    Scope = ComponentVisibilityScope.PUBLIC,
                    NextToken = nextToken
                });

                publicComponents.AddRange(response.Components);

                nextToken = response.NextToken;
            }
            while (!string.IsNullOrEmpty(nextToken));

            return publicComponents.Select(c => new IoTEdgeModule
            {
                Id = c.Arn,
                ModuleName = c.ComponentName,
                Version = c.LatestVersion.ComponentVersion,
                // ImageURI is required, but not used for Greengrass components
                ImageURI = "example.com"
            });
        }
    }
}
