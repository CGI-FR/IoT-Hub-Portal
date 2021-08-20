// Copyright (c) CGI France - Grand Est. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Server.Filters;
    using AzureIoTHub.Portal.Shared.Models;
    using AzureIoTHub.Portal.Shared.Security;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.Devices;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    [Authorize]
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = RoleNames.Admin)]
    public class GatewaysController : ControllerBase
    {
        private readonly ILogger<GatewaysController> logger;

        private readonly RegistryManager registryManager;

        public GatewaysController(ILogger<GatewaysController> logger, RegistryManager registryManager)
        {
            this.logger = logger;
            this.registryManager = registryManager;
        }

        [HttpGet]
        public async Task<IEnumerable<Gateway>> Get()
        {
            try
            {
                var query = this.registryManager.CreateQuery("SELECT * FROM devices where devices.capabilities.iotEdge = true", 10);
                var items = await query.GetNextAsTwinAsync();
                List<Gateway> gatewayList = new ();

                foreach (var gateway in items)
                {
                    var typeGate = "unknow";
                    var environementGate = "unknow";

                    if (gateway.Tags.Count > 1)
                    {
                        if (gateway.Tags["type"] != null)
                        {
                            typeGate = gateway.Tags["type"];
                        }

                        if (gateway.Tags["env"] != null)
                        {
                            environementGate = gateway.Tags["env"];
                        }
                    }

                    gatewayList.Add(new Gateway
                    {
                        DeviceID = gateway.DeviceId,
                        Status = gateway.Status.Value.ToString(),
                        TypeOfGateway = typeGate,
                        Environement = environementGate
                    });
                }

                return gatewayList;
                // return items.Select(c => new GatewayListItem
                // {
                //    ID = c.DeviceId,
                //    Type = c.AuthenticationType.Value.ToString(),
                //    Status = c.Status.Value.ToString(),
                //    Tags = c.Tags
                // });
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet("deviceID")]
        public async Task<Gateway> Get(string deviceID)
        {
            var device = await this.registryManager.GetTwinAsync(deviceID);
            return new Gateway
            {
                DeviceID = device.DeviceId,
                Status = device.Status.Value.ToString(),
                TypeOfGateway = device.Tags["type"]
            };
        }

        //// POST: GatewaysController/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Create(IFormCollection collection)
        // {
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        // }

        //// POST: GatewaysController/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Edit(int id, IFormCollection collection)
        // {
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        // }

        //// POST: GatewaysController/Delete/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Delete(int id, IFormCollection collection)
        // {
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        // }
    }
}
