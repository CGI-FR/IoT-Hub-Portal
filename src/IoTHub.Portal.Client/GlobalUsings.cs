// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
global using IoTHub.Portal.Settings;
global using IoTHub.Portal.Shared.Models.v1._0;
global using IoTHub.Portal.Shared.Models.v10;
global using Microsoft.AspNetCore.Components.Web;
global using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
global using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
global using Microsoft.JSInterop;
global using MudBlazor;
global using MudBlazor.Services;
global using Toolbelt.Blazor.Extensions.DependencyInjection;
global using Tewr.Blazor.FileReader;
global using IoTHub.Portal.Shared.Models;
global using IoTHub.Portal.Client.Validators;
global using System.Text;
global using System.Text.Json;

global using System.Net;
global using IoTHub.Portal.Shared.Models.v10.Filters;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.WebUtilities;
global using IoTHub.Portal.Shared.Models.v10.LoRaWAN;
global using IoTHub.Portal.Shared.Constants;
