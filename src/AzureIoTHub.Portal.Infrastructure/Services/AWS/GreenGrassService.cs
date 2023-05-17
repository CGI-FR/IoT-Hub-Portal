// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Services.AWS
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon.GreengrassV2;
    using Amazon.GreengrassV2.Model;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Application.Services;
    using AzureIoTHub.Portal.Domain;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Domain.Exceptions;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Models.v10;
    using AzureIoTHub.Portal.Shared.Models.v10.Filters;
    using Microsoft.AspNetCore.Http;
    using System.ComponentModel;

    public class GreenGrassService : IEdgeModelService
    {
        private readonly IAmazonGreengrassV2 greengras;
        private readonly IDeviceModelImageManager deviceModelImageManager;
        private readonly IMapper mapper;
        private readonly IEdgeDeviceModelRepository edgeModelRepository;
        private readonly IUnitOfWork unitOfWork;

        public GreenGrassService(
            IAmazonGreengrassV2 greengras,
            IDeviceModelImageManager deviceModelImageManager,
            IMapper mapper,
            IEdgeDeviceModelRepository edgeModelRepository,
            IUnitOfWork unitOfWork)
        {
            this.greengras = greengras;
            this.deviceModelImageManager = deviceModelImageManager;
            this.mapper = mapper;
            this.edgeModelRepository = edgeModelRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task CreateGreenGrassDeployment(IoTEdgeModel edgeModel)
        {
            var createDeploymentRequest = new CreateDeploymentRequest
            {
                DeploymentName = edgeModel?.Name,
                Components = await CreateGreenGrassComponents(edgeModel),
                TargetArn = "arn:aws:iot:eu-west-1:578920151383:thinggroup/test" //How?
            };

            var createDeploymentResponse = await this.greengras.CreateDeploymentAsync(createDeploymentRequest);

            if (createDeploymentResponse.HttpStatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new InternalServerErrorException("The creation of the deployment failed due to an error in the Amazon IoT API.");

            }

            //Must add the version column
            var edgeModelEntity = this.mapper.Map<EdgeDeviceModel>(edgeModel);
            edgeModelEntity.Id = createDeploymentResponse.DeploymentId;

            await this.edgeModelRepository.InsertAsync(edgeModelEntity);
            await this.unitOfWork.SaveAsync();
            _ = await this.deviceModelImageManager.SetDefaultImageToModel(edgeModelEntity.Id);

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
                    throw new InternalServerErrorException("The creation of the component failed due to an error in the Amazon IoT API.");

                }
                listcomponentName.Add(response.ComponentName, new ComponentDeploymentSpecification { ComponentVersion = "1.0.0" });
            }

            return listcomponentName;

        }
        private static JObject JsonCreateComponent(IoTEdgeModule component)
        {
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
                                                new JProperty("Environment",
                                                    new JObject(
                                                        new JProperty("VAR1", "value1"),
                                                        new JProperty("VAR2", "value2")
                                                    ))
                                   )),
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
        public Task CreateEdgeModel(IoTEdgeModel edgeModel)
        {
            throw new NotImplementedException();
        }
        public Task DeleteEdgeModel(string edgeModelId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEdgeModelAvatar(string edgeModelId)
        {
            throw new NotImplementedException();
        }

        public Task<IoTEdgeModel> GetEdgeModel(string modelId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEdgeModelAvatar(string edgeModelId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IoTEdgeModelListItem>> GetEdgeModels(EdgeModelFilter edgeModelFilter)
        {
            throw new NotImplementedException();
        }

        public Task SaveModuleCommands(IoTEdgeModel deviceModelObject)
        {
            throw new NotImplementedException();
        }

        public Task UpdateEdgeModel(IoTEdgeModel edgeModel)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpdateEdgeModelAvatar(string edgeModelId, IFormFile file)
        {
            throw new NotImplementedException();
        }
    }
}
