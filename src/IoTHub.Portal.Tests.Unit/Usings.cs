// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

global using System;
global using System.Collections.Generic;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Linq.Expressions;
global using System.Net;
global using System.Net.Http;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;

global using AutoFixture;
global using AutoMapper;
global using Azure.Data.Tables;
global using FluentAssertions;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Azure.Devices;
global using Microsoft.Azure.Devices.Shared;
global using Microsoft.Azure.Devices.Provisioning.Service;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Moq;
global using NUnit.Framework;

global using IoTHub.Portal.Application.Helpers;
global using IoTHub.Portal.Application.Managers;
global using IoTHub.Portal.Application.Services;
global using IoTHub.Portal.Application.Wrappers;
global using IoTHub.Portal.Crosscutting.Extensions;
global using IoTHub.Portal.Domain;
global using IoTHub.Portal.Domain.Entities;
global using IoTHub.Portal.Domain.Exceptions;
global using IoTHub.Portal.Domain.Options;
global using IoTHub.Portal.Domain.Repositories;
global using IoTHub.Portal.Domain.Shared;
global using IoTHub.Portal.Domain.Shared.Constants;
global using IoTHub.Portal.Infrastructure;
global using IoTHub.Portal.Infrastructure.Mappers;
global using IoTHub.Portal.Infrastructure.Providers;
global using IoTHub.Portal.Infrastructure.Services;
global using IoTHub.Portal.Models.v10;
global using IoTHub.Portal.Models.v10.LoRaWAN;
global using IoTHub.Portal.Shared.Models.v10;
global using IoTHub.Portal.Shared.Models.v1._0;
global using IoTHub.Portal.Shared.Models.v10.Filters;
global using IoTHub.Portal.Tests.Unit.UnitTests.Bases;
