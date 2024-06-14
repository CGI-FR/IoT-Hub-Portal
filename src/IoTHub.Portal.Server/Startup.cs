// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Server
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using IoTHub.Portal.Application.Managers;
    using IoTHub.Portal.Application.Services;
    using IoTHub.Portal.Application.Startup;
    using IoTHub.Portal.Domain.Shared.Constants;
    using IoTHub.Portal.Infrastructure.Services;
    using IoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using IoTHub.Portal.Infrastructure.Startup;
    using IoTHub.Portal.Shared.Constants;
    using Domain;
    using Domain.Exceptions;
    using EntityFramework.Exceptions.Common;
    using Extensions;
    using Hellang.Middleware.ProblemDetails;
    using Hellang.Middleware.ProblemDetails.Mvc;
    using Identity;
    using Infrastructure;
    using Managers;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Primitives;
    using Microsoft.OpenApi.Models;
    using MudBlazor.Services;
    using Prometheus;
    using Quartz;
    using Quartz.Impl.AdoJobStore.Common;
    using Services;
    using Shared.Models.v10;

    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            HostEnvironment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostEnvironment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services, nameof(services));

            var configuration = ConfigHandlerFactory.Create(HostEnvironment, Configuration);

            _ = services.AddSingleton(configuration);

            _ = services.AddInfrastructureLayer(configuration)
                        .AddApplicationLayer();

            AddAuthenticationAndAuthorization(services, configuration);

            //CloudProvider-dependant configurations
            switch (configuration.CloudProvider)
            {
                case CloudProviders.Azure:
                    ConfigureServicesAzure(services);
                    break;
                case CloudProviders.AWS:
                    ConfigureServicesAws(services);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid CloudProvider setting: '{configuration.CloudProvider}'. Only '{CloudProviders.Azure}' and '{CloudProviders.AWS}' are supported.");
            }

            //Common configurations
            _ = services.AddSingleton(new PortalMetric());
            _ = services.AddSingleton(new LoRaGatewayIDList());

            _ = services.AddRazorPages();
            _ = services.AddMudServices();

            ConfigureIdeasFeature(services, configuration);

            // Add problem details support
            _ = services.AddProblemDetails(setup =>
            {
                setup.IncludeExceptionDetails = (_, _) => HostEnvironment.IsDevelopment();

                // Custom mapping function for FluentValidation's ValidationException.
                setup.MapFluentValidationException();

                setup.Map<InternalServerErrorException>(exception => new ProblemDetails
                {
                    Title = exception.Title,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status500InternalServerError
                });

                setup.Map<ResourceNotFoundException>(exception => new ProblemDetails
                {
                    Title = exception.Title,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status404NotFound
                });

                setup.Map<BaseException>(exception => new ProblemDetails
                {
                    Title = exception.Title,
                    Detail = exception.Detail,
                    Status = StatusCodes.Status400BadRequest
                });

                setup.Map<ArgumentNullException>(exception => new ProblemDetails
                {
                    Title = "Null Argument",
                    Detail = exception.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["params"] = exception.ParamName
                    }
                });

                setup.Map<UniqueConstraintException>(exception => new ProblemDetails
                {
                    Title = "Unique Violation",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions =
                    {
                        ["params"] = exception.Entries.ToString()
                    }
                });

                setup.Map<CannotInsertNullException>(exception => new ProblemDetails
                {
                    Title = "Not Null Violation",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions =
                    {
                        ["params"] = exception.Entries.ToString(),
                    }
                });

                setup.Map<MaxLengthExceededException>(exception => new ProblemDetails
                {
                    Title = "String Data Right Truncation",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions =
                    {
                        ["params"] = exception.Entries.ToString()
                    }
                });

                setup.Map<NumericOverflowException>(exception => new ProblemDetails
                {
                    Title = "Numeric Value Out Of Range",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions =
                    {
                        ["params"] = exception.Entries.ToString()
                    }
                });

                setup.Map<ReferenceConstraintException>(exception => new ProblemDetails
                {
                    Title = "Foreign Key Violation",
                    Detail = exception.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Extensions =
                    {
                        ["params"] = exception.Entries.ToString()
                    }
                });
            });

            _ = services.AddControllers();
            _ = services.AddProblemDetailsConventions();

            _ = services.AddEndpointsApiExplorer();

            _ = services.AddApplicationInsightsTelemetry();

            _ = services.AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "1.0",
                    Title = "Azure IoT Hub Portal API",
                    Description = "Available APIs for managing devices from Azure IoT Hub."
                });

                // using System.Reflection;
                opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "IoTHub.Portal.Server.xml"));
                opts.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "IoTHub.Portal.Shared.xml"));

                opts.TagActionsBy(api => new[] { api.GroupName });
                opts.DocInclusionPredicate((_, _) => true);

                opts.DescribeAllParametersInCamelCase();

                var securityDefinition = new OpenApiSecurityScheme
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description =
  @"
    Specify the authorization token got from your IDP as a header.
    > Ex: ``Authorization: Bearer * ***``",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                };

                var securityRequirements = new OpenApiSecurityRequirement
                {
                    { securityDefinition, Array.Empty<string>() },
                  };

                opts.AddSecurityDefinition("Bearer", securityDefinition);

                opts.AddSecurityRequirement(securityRequirements);

                opts.OrderActionsBy(api => api.RelativePath);
                opts.UseInlineDefinitionsForEnums();
            });

            _ = services.AddApiVersioning(o =>
            {
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("api-version"),
                    new HeaderApiVersionReader("X-Version"));
            });

            // Add the required Quartz.NET services
            _ = services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.InterruptJobsOnShutdownWithWait = true;


                q.UsePersistentStore(opts =>
                {
                    // JSON is recommended persistent format to store data in database for greenfield projects.
                    // You should also strongly consider setting useProperties to true to restrict key - values to be strings.
                    opts.UseJsonSerializer();
                    opts.UseClustering();
                    opts.UseProperties = true;
                    opts.PerformSchemaValidation = false;

                    switch (configuration.DbProvider)
                    {
                        case DbProviders.PostgreSQL:
                            opts.UsePostgres(c =>
                            {
                                c.ConnectionString = configuration.PostgreSQLConnectionString;
                            });
                            break;
                        case DbProviders.MySQL:
                            DbProvider.RegisterDbMetadata("IotHubPortalQuartzMySqlConnector", new DbMetadata()
                            {
                                ProductName = "MySQL, MySqlConnector provider",
                                AssemblyName = "MySqlConnector",
                                ConnectionType = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector"),
                                CommandType = Type.GetType("MySqlConnector.MySqlCommand, MySqlConnector"),
                                ParameterType = Type.GetType("MySqlConnector.MySqlParameter, MySqlConnector"),
                                ParameterDbType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector"),
                                ParameterDbTypePropertyName = "MySqlDbType",
                                ParameterNamePrefix = "?",
                                ExceptionType = Type.GetType("MySqlConnector.MySqlException, MySqlConnector"),
                                UseParameterNamePrefixInParameterCollection = true,
                                BindByName = true,
                                DbBinaryTypeName = "Blob"
                            });

                            opts.UseGenericDatabase("IotHubPortalQuartzMySqlConnector", c =>
                            {
                                c.ConnectionString = configuration.MySQLConnectionString;
                            });
                            break;
                        default:
                            break;
                    }
                });
            });

            // Add the Quartz.NET hosted service
            _ = services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            _ = services.AddDataProtection()
                .PersistKeysToDbContext<PortalDbContext>();
        }

        private static void ConfigureServicesRBAC(IServiceCollection services)
        {
            _ = services.AddTransient<IRoleManagementService, RoleService>();
            _ = services.AddTransient<IGroupManagementService, GroupService>();
            _ = services.AddTransient<IUserManagementService, UserService>();
            _ = services.AddTransient<IAccessControlManagementService, AccessControlService>();
        }
        private static void ConfigureServicesAzure(IServiceCollection services)
        {
            _ = services.AddTransient<IExportManager, ExportManager>();
            _ = services.AddTransient<IConfigService, ConfigService>();
            _ = services.AddTransient<ILoRaWANCommandService, LoRaWANCommandService>();
            _ = services.AddTransient<IEdgeModelService, EdgeModelService>();
            _ = services.AddTransient<ILoRaWANConcentratorService, LoRaWANConcentratorService>();
            _ = services.AddTransient<IEdgeDevicesService, AzureEdgeDevicesService>();
            _ = services.AddTransient<IExternalDeviceService, ExternalDeviceService>();
            _ = services.AddTransient<IDevicePropertyService, DevicePropertyService>();
            _ = services.AddTransient<IDeviceConfigurationsService, DeviceConfigurationsService>();
            _ = services.AddTransient(typeof(IDeviceModelService<,>), typeof(DeviceModelService<,>));
        }

        private static void ConfigureServicesAws(IServiceCollection services)
        {
            _ = services.AddTransient<IEdgeDevicesService, AWSEdgeDevicesService>();
        }

        private static void ConfigureIdeasFeature(IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.AddTransient<IIdeaService, IdeaService>();

            _ = services.AddHttpClient<IIdeaService, IdeaService>(client =>
            {
                if (!configuration.IdeasEnabled) return;
                client.BaseAddress = new Uri(configuration.IdeasUrl);
                client.DefaultRequestHeaders.Add(configuration.IdeasAuthenticationHeader,
                    configuration.IdeasAuthenticationToken);
            });
        }

        private static void AddAuthenticationAndAuthorization(IServiceCollection services, ConfigHandler configuration)
        {
            _ = services.Configure<ClientApiIndentityOptions>(opts =>
            {
                opts.MetadataUrl = new Uri(configuration.OIDCMetadataUrl);
                opts.ClientId = configuration.OIDCClientId;
                opts.Scope = configuration.OIDCScope;
                opts.Authority = configuration.OIDCAuthority;
            });

            _ = services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(opts =>
                {
                    opts.Authority = configuration.OIDCAuthority;
                    opts.MetadataAddress = configuration.OIDCMetadataUrl;
                    opts.Audience = configuration.OIDCApiClientId;

                    opts.TokenValidationParameters.ValidateIssuer = configuration.OIDCValidateIssuer;
                    opts.TokenValidationParameters.ValidateAudience = configuration.OIDCValidateAudience;
                    opts.TokenValidationParameters.ValidateLifetime = configuration.OIDCValidateLifetime;
                    opts.TokenValidationParameters.ValidateIssuerSigningKey = configuration.OIDCValidateIssuerSigningKey;
                    opts.TokenValidationParameters.ValidateActor = configuration.OIDCValidateActor;
                    opts.TokenValidationParameters.ValidateTokenReplay = configuration.OIDCValidateTokenReplay;
                });
            ConfigureServicesRBAC(services);

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ArgumentNullException.ThrowIfNull(env, nameof(env));
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var configuration = app.ApplicationServices.GetService<ConfigHandler>();

            // Use problem details
            _ = app.UseProblemDetails();
            app.UseIfElse(IsApiRequest, UseApiExceptionMiddleware, UseUIExceptionMiddleware);

            if (configuration.UseSecurityHeaders)
            {
                _ = app.UseSecurityHeaders(opts =>
                {
                    _ = opts.AddContentSecurityPolicy(csp =>
                    {
                        _ = csp.AddFrameAncestors()
                            .Self()
                            .From(configuration.OIDCMetadataUrl);
                    });
                });
            }

            if (env.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
                _ = app.UseSwagger();
                _ = app.UseSwaggerUI(options =>
                {
                    // Disable swagger "Try It Out" feature
                    options.SupportedSubmitMethods();
                });
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                _ = app.UseHsts();
            }

            _ = app.UseHttpsRedirection();
            _ = app.UseBlazorFrameworkFiles();
            _ = app.UseStaticFiles();

            _ = app.UseRouting();

            _ = app.UseAuthentication();
            _ = app.UseAuthorization();
            _ = app.UseMetricServer();

            _ = app.UseEndpoints(endpoints =>
            {
                _ = endpoints.MapRazorPages();
                _ = endpoints.MapControllers();
                // endpoints.MapFallbackToFile("index.html");

                // Prevent the user from getting HTML when the controller can't be found.
#pragma warning disable ASP0018 // Unused route parameter
                _ = endpoints.Map("api/{**slug}", HandleApiFallback);
#pragma warning restore ASP0018 // Unused route parameter

                // If this is a request for a web page, just do the normal out-of-the-box behaviour.
                _ = endpoints.MapFallbackToFile("{**slug}", "index.html", new StaticFileOptions
                {
                    OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", new StringValues("no-cache"))
                });

                _ = endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    ResponseWriter = HealthCheckResponseWriter.WriteHealthReport
                });
            });


            //CloudProvider-dependant configurations
            switch (configuration.CloudProvider)
            {
                case CloudProviders.Azure:
                    await ConfigureAzureAsync(app);
                    break;
                case CloudProviders.AWS:
                    await ConfigureAwsAsync(app);
                    break;
                default:
                    break;
            }
        }

        private static async Task ConfigureAzureAsync(IApplicationBuilder app)
        {
            var deviceModelImageManager = app.ApplicationServices.GetService<IDeviceModelImageManager>();

            await deviceModelImageManager?.InitializeDefaultImageBlob()!;
            await deviceModelImageManager?.SyncImagesCacheControl()!;
        }
        private static async Task ConfigureAwsAsync(IApplicationBuilder app)
        {
            var deviceModelImageManager = app.ApplicationServices.GetService<IDeviceModelImageManager>();

            await deviceModelImageManager?.InitializeDefaultImageBlob()!;
        }

        private static void UseApiExceptionMiddleware(IApplicationBuilder app)
        {
            _ = app.UseProblemDetails();
        }

        private void UseUIExceptionMiddleware(IApplicationBuilder app)
        {
            _ = HostEnvironment.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseExceptionHandler("/Error");
        }

        private static bool IsApiRequest(HttpContext httpContext)
        {
            return httpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        }

        private Task HandleApiFallback(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        }
    }
}
