// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Domain
{
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Domain.Repositories;

    public interface IUnitOfWork
    {
        IDeviceModelRepository DeviceModelRepository { get; }
        ILabelRepository LabelRepository { get; }

        Task SaveAsync();
    }
}
