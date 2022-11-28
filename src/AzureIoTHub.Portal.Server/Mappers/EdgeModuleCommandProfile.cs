// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;

    public class EdgeModuleCommandProfile : Profile
    {
        public EdgeModuleCommandProfile()
        {
            _ = CreateMap<EdgeModuleCommand, EdgeModuleCommand>();

            _ = CreateMap<EdgeModuleCommandDto, EdgeModuleCommand>();

            _ = CreateMap<EdgeModuleCommand, EdgeModuleCommandDto>();

            _ = CreateMap<EdgeModuleCommandPayload, EdgeModuleCommandPayloadDto>();
        }
    }
}
