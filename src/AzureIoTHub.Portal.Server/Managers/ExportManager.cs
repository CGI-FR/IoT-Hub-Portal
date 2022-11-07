// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Managers
{
    using System.IO;
    using System.Threading.Tasks;
    using System;
    using AzureIoTHub.Portal.Infrastructure;
    using AzureIoTHub.Portal.Server.Services;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Text;

    public class ExportManager : IExportManager
    {
        private readonly PortalDbContext portalDbContext;
        private readonly IExternalDeviceService externalDevicesService;
        private readonly IDeviceTagService deviceTagService;

        public ExportManager(PortalDbContext portalDbContext,
                                IExternalDeviceService externalDevicesService,
                                IDeviceTagService deviceTagService)
        {
            this.portalDbContext = portalDbContext;
            this.externalDevicesService = externalDevicesService;
            this.deviceTagService = deviceTagService;
        }


        public async Task<Stream> ExportDeviceList()
        {
            var tags = this.deviceTagService.GetAllTagsNames();
            var query = this.portalDbContext.Devices
                .Include(device => device.Tags);
            var devices = await query.ToListAsync();

            var textContent = "Id,Name,DeviceModelId,IsEnabled,Version";
            foreach (var tag in tags)
            {
                textContent += $",TAG:{tag}";
            }

            foreach (var device in devices)
            {
                textContent += $"\n{device.Id},{device.Name},{device.DeviceModelId},{device.IsEnabled},{device.Version}";
                foreach (var tag in tags)
                {
                    var value = device.Tags.Where(x => x.Name == tag).Select(x => x.Value).SingleOrDefault();
                    textContent += $",{value}";
                }
            }

            var textAsBytes = Encoding.Unicode.GetBytes(textContent);
            var stream = new MemoryStream(textAsBytes);
            return stream;
        }
    }
}
