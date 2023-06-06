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
    using System.Text.RegularExpressions;

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
                await CreateNonExisitingGreenGrassDeployment(deployment);
            }

            //Delete in DB AWS deleted deployments
            await DeleteGreenGrassDeployments(awsGreenGrassDeployments);
        }

        private async Task<List<IoTEdgeModel>> GetAllGreenGrassDeployments()
        {
            var deployments = new List<IoTEdgeModel>();

            var nextToken = string.Empty;

            do
            {
                var request = new ListDeploymentsRequest
                {
                    NextToken = nextToken,
                    HistoryFilter = DeploymentHistoryFilter.LATEST_ONLY
                };

                var response = await this.amazonGreenGrass.ListDeploymentsAsync(request);

                foreach (var deployment in response.Deployments)
                {
                    var awsThingGroupRegex = new Regex(@"/([^/]+)$");
                    var matches = awsThingGroupRegex.Match(deployment.TargetArn);

                    if (matches.Success && matches.Groups.Count > 1)
                    {
                        var thinggroupName = matches.Groups[1].Value;
                        var s = await this.amazonIoTClient.DescribeThingGroupAsync(new Amazon.IoT.Model.DescribeThingGroupRequest { ThingGroupName = thinggroupName });
                        if (s.QueryString != null)
                        {
                            var iotEdgeModel = new IoTEdgeModel
                            {
                                ModelId = deployment.DeploymentId, //Instead of giving a random Id here, we can give the deploymentID
                                Name = deployment.DeploymentName,
                                ExternalIdentifier = deployment.DeploymentId
                            };
                            deployments.Add(iotEdgeModel);
                        }
                    }
                }

                nextToken = response.NextToken;
            }
            while (!string.IsNullOrEmpty(nextToken));

            return deployments;
        }

        private async Task CreateNonExisitingGreenGrassDeployment(IoTEdgeModel iotEdgeModel)
        {
            var iotEdgeModels = (await this.edgeDeviceModelRepository.GetAllAsync())
                .Where(edge => string.Equals(edge.ExternalIdentifier, iotEdgeModel.ExternalIdentifier, StringComparison.Ordinal)).ToList();

            if (iotEdgeModels.Count == 0)
            {
                //In Aws, it is possible to create a deployment without a name, so it will take the id as a name
                //Here is how we handle it
                iotEdgeModel.Name ??= iotEdgeModel.ModelId;

                var edgeModel = this.mapper.Map<EdgeDeviceModel>(iotEdgeModel);

                await this.edgeDeviceModelRepository.InsertAsync(edgeModel);
                await this.unitOfWork.SaveAsync();
                _ = this.deviceModelImageManager.SetDefaultImageToModel(edgeModel.Id);
            }

        }

        private async Task DeleteGreenGrassDeployments(List<IoTEdgeModel> edgeModels)
        {
            //Get All Deployments that are not in AWS
            var deploymentToDelete = (await this.edgeDeviceModelRepository.GetAllAsync())
                .Where(edge => !edgeModels.Any(edgeModel => string.Equals(edge.ExternalIdentifier, edgeModel.ExternalIdentifier, StringComparison.Ordinal)))
                .ToList();

            foreach (var edgeModel in deploymentToDelete)
            {
                await this.deviceModelImageManager.DeleteDeviceModelImageAsync(edgeModel.Id);
                this.edgeDeviceModelRepository.Delete(edgeModel.Id);
                await this.unitOfWork.SaveAsync();
            }
        }
    }
}
