// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using System;
global using System.Linq;
global using System.Net.Http;
global using System.Net.Http.Json;
global using Blazored.LocalStorage;
global using FluentValidation;
global using IoTHub.Portal.Client;
global using IoTHub.Portal.Client.Constants;
global using IoTHub.Portal.Client.Dialogs.Planning;
global using IoTHub.Portal.Client.Exceptions;
global using IoTHub.Portal.Client.Handlers;
global using IoTHub.Portal.Client.Models;
global using IoTHub.Portal.Client.Services;
global using IoTHub.Portal.Models.v10;
global using IoTHub.Portal.Models.v10.LoRaWAN;
global using IoTHub.Portal.Shared;
global using IoTHub.Portal.Shared.Constants;
global using IoTHub.Portal.Shared.Models;
global using IoTHub.Portal.Shared.Models.v10;
global using IoTHub.Portal.Shared.Models.v10.Filters;
global using IoTHub.Portal.Shared.Models.v10.LoRaWAN;
global using IoTHub.Portal.Shared.Settings;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.JSInterop;
global using MudBlazor;
global using MudBlazor.Services;
global using Toolbelt.Blazor.Extensions.DependencyInjection;
global using Tewr.Blazor.FileReader;
global using System.Text;
global using System.Text.Json;

global using System.Net;
global using System.Threading.Tasks;
global using IoTHub.Portal.Shared.Security;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Authorization;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.Extensions.Logging;