// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Infrastructure.Jobs.AWS
{
    using Amazon.IoT;
    using AutoMapper;
    using AzureIoTHub.Portal.Application.Managers;
    using AzureIoTHub.Portal.Domain.Repositories;
    using AzureIoTHub.Portal.Domain;
    using Microsoft.Extensions.Logging;
    using Quartz;
    using Amazon.GreengrassV2;
    using AzureIoTHub.Portal.Models.v10;
    using Amazon.GreengrassV2.Model;
    using AzureIoTHub.Portal.Domain.Entities;

    [DisallowConcurrentExecution]
    public class SyncGreenGrassDeploymentsJob : IJob
    {
        private readonly ILogger<SyncThingTypesJob> logger;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly IEdgeDeviceModelRepository edgeDeviceModelRepository;
        private readonly IAmazonIoT amazonIoTClient;
        private readonly IAmazonGreengrassV2 amazonGreenGrass;
        private readonly IDeviceModelImageManager deviceModelImageManager;

        public SyncGreenGrassDeploymentsJob(
            ILogger<SyncThingTypesJob> logger,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            IEdgeDeviceModelRepository edgeDeviceModelRepository,
            IAmazonIoT amazonIoTClient,
            IAmazonGreengrassV2 amazonGreenGrass,
            IDeviceModelImageManager awsImageManager)
        {
            this.deviceModelImageManager = awsImageManager;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.edgeDeviceModelRepository = edgeDeviceModelRepository;
            this.amazonIoTClient = amazonIoTClient;
            this.amazonGreenGrass = amazonGreenGrass;
            this.logger = logger;
        }


        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                this.logger.LogInformation("Start of sync Greengrass Deployents job");

                await SyncGreenGrassDeployments();

                this.logger.LogInformation("End of sync Greengrass Deployents job");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Sync Greengrass Deployents job has failed");
            }
        }

        private async Task SyncGreenGrassDeployments()
        {
            var awsGreenGrassDeployments = await GetAllGreenGrassDeployments();

            foreach (var deployment in awsGreenGrassDeployments)
            {
                await CreateOrUpdateGreenGrassDeployment(deployment);
            }
        }

        private async Task<List<IoTEdgeModel>> GetAllGreenGrassDeployments()
        {
            var deployments = new List<IoTEdgeModel>();

            var nextToken = string.Empty;

            var getAllAwsGreenGrassDeployments = await this.amazonGreenGrass.ListDeploymentsAsync(
                new ListDeploymentsRequest
                {
                    NextToken = nextToken,
                });

            foreach (var deployment in getAllAwsGreenGrassDeployments.Deployments)
            {
                var iotEdgeModel = new IoTEdgeModel
                {
                    ModelId = deployment.DeploymentId, //Instead of giving a random Id here, we can give the deploymentID
                    Name = deployment.DeploymentName,
                    ExternalIdentifier = deployment.DeploymentId
                };
                deployments.Add(iotEdgeModel);
            }
            return deployments;
        }

        private async Task CreateOrUpdateGreenGrassDeployment(IoTEdgeModel iotEdgeModel)
        {

            var iotEdgeModels = (await this.edgeDeviceModelRepository.GetAllAsync()).Where(edge => edge.ExternalIdentifier == iotEdgeModel.ExternalIdentifier).ToList();
            if (iotEdgeModels.Count == 0)
            {
                var edgeModel = this.mapper.Map<EdgeDeviceModel>(iotEdgeModel);

                await this.edgeDeviceModelRepository.InsertAsync(edgeModel);
                await this.unitOfWork.SaveAsync();
                _ = this.deviceModelImageManager.SetDefaultImageToModel(edgeModel.Id);
            }

        }

        /*private async Task DeleteGreenGrassDeployments(List<IoTEdgeModel> edgeModels)
        {

        }*/
    }
}
