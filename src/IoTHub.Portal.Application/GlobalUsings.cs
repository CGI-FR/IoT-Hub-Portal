// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Runtime.CompilerServices;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using Amazon.IoT.Model;
global using Amazon.IotData.Model;
global using AutoMapper;
global using Azure;
global using Azure.Messaging.EventHubs;
global using IoTHub.Portal.Application.Helpers;
global using IoTHub.Portal.Crosscutting.Extensions;
global using IoTHub.Portal.Domain.Entities;
global using IoTHub.Portal.Domain.Shared;
global using IoTHub.Portal.Models.v10;
global using IoTHub.Portal.Models.v10.LoRaWAN;
global using IoTHub.Portal.Shared.Models;
global using IoTHub.Portal.Shared.Models.v1._0;
global using IoTHub.Portal.Shared.Models.v10;
global using IoTHub.Portal.Shared.Models.v10.Filters;
global using IoTHub.Portal.Shared.Models.v10.IoTEdgeModule;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Azure.Devices;
global using Microsoft.Azure.Devices.Provisioning.Service;
global using Microsoft.Azure.Devices.Shared;
global using Microsoft.Extensions.DependencyInjection;
global using Newtonsoft.Json.Linq;
global using Stream = System.IO.Stream;
