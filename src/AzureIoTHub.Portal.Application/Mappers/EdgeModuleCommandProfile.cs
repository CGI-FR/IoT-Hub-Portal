// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Mappers
{
    using AutoMapper;
    using AzureIoTHub.Portal.Domain.Entities;
    using AzureIoTHub.Portal.Shared.Models.v1._0.IoTEdgeModuleCommand;
    using Newtonsoft.Json;

    public class EdgeModuleCommandProfile : Profile
    {
        public EdgeModuleCommandProfile()
        {
            _ = CreateMap<EdgeModuleCommand, EdgeModuleCommand>();

            _ = CreateMap<EdgeModuleCommandDto, EdgeModuleCommand>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.Type))
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Comment, opts => opts.MapFrom(src => src.Comment))
                .ForMember(dest => dest.CommandType, opts => opts.MapFrom(src => src.CommandType));

            _ = CreateMap<EdgeModuleCommand, EdgeModuleCommandDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.Type, opts => opts.MapFrom(src => src.Type))
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.Comment, opts => opts.MapFrom(src => src.Comment))
                .ForMember(dest => dest.CommandType, opts => opts.MapFrom(src => src.CommandType));

            _ = CreateMap<EdgeModuleCommandPayloadDto, EdgeModuleCommandPayload>()
                .ForMember(dest => dest.EdgeModuleCommandId, opts => opts.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.ComplexSchema, opts => opts.MapFrom(src => src.ComplexSchema.ToString()))
                .ForMember(dest => dest.moduleCommandSchemaType, opts => opts.MapFrom(src => src.SchemaType))
                .ForMember(dest => dest.Comment, opts => opts.MapFrom(src => src.Comment))
                .ForMember(dest => dest.InitialValue, opts => opts.MapFrom(src => src.InitialValue));

            _ = CreateMap<EdgeModuleCommandPayload, EdgeModuleCommandPayloadDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(src => src.EdgeModuleCommandId))
                .ForMember(dest => dest.Name, opts => opts.MapFrom(src => src.Name))
                .ForMember(dest => dest.DisplayName, opts => opts.MapFrom(src => src.DisplayName))
                .ForMember(dest => dest.Description, opts => opts.MapFrom(src => src.Description))
                .ForMember(dest => dest.ComplexSchema, opts => opts.MapFrom(src => TryToDeserializeObjectSchema(src.ComplexSchema)))
                .ForMember(dest => dest.SchemaType, opts => opts.MapFrom(src => src.moduleCommandSchemaType))
                .ForMember(dest => dest.Comment, opts => opts.MapFrom(src => src.Comment))
                .ForMember(dest => dest.InitialValue, opts => opts.MapFrom(src => src.InitialValue));
        }

        private static object TryToDeserializeObjectSchema(object schema)
        {
            try
            {
                return JsonConvert.DeserializeObject<PayloadDataTypeDto>(schema?.ToString());
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }
    }
}
