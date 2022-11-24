// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.Factories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using NUnit.Framework;

    [TestFixture]
    public class TableClientFactoryTests
    {
        private const string ContainerName = "azurite";
        private const string ImageName = "mcr.microsoft.com/azure-storage/azurite";
        private const string ImageTag = "latest";
        private static readonly string TestContainerName = ContainerName + "AzureIoTHub";

        private string containerId;



        [OneTimeSetUp]
        protected void SetUp()
        {
            IList<ContainerListResponse> containers = new List<ContainerListResponse>();
            var dockerConnection = Environment.OSVersion.Platform.ToString().Contains("Win", StringComparison.Ordinal) ?
                    "npipe://./pipe/docker_engine" :
                    "unix:///var/run/docker.sock";
            Console.WriteLine("Starting container");
            using var conf = new DockerClientConfiguration(new Uri(dockerConnection)); // localhost
            using var client = conf.CreateClient();

            try
            {

                Console.WriteLine("On Premise execution detected");
                Console.WriteLine("Starting container...");
                containers = client.Containers.ListContainersAsync(new ContainersListParameters() { All = true }).GetAwaiter().GetResult();
                Console.WriteLine("listing container...");

                // Download image only if not found
                try
                {
                    _ = client.Images.InspectImageAsync(ImageName).GetAwaiter().GetResult();
                }
                catch (DockerImageNotFoundException)
                {
                    client.Images.CreateImageAsync(new ImagesCreateParameters() { FromImage = ImageName, Tag = ImageTag }, new AuthConfig(), new Progress<JSONMessage>()).GetAwaiter().GetResult();
                }

                // Create the container
                var config = new Config()
                {
                    Hostname = "localhost"
                };

                // Configure the ports to expose
                var hostConfig = new HostConfig()
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {
                            $"10000/tcp", new List<PortBinding> { new PortBinding { HostIP = "127.0.0.1", HostPort = "10000" } }
                        },
                        {
                            $"10001/tcp", new List<PortBinding> { new PortBinding { HostIP = "127.0.0.1", HostPort = "10001" } }
                        },
                        {
                            $"10002/tcp", new List<PortBinding> { new PortBinding { HostIP = "127.0.0.1", HostPort = "10002" } }
                        }
                    }
                };

                Console.WriteLine("Creating container...");

                // Create the container
                var response = client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
                {
                    Image = ImageName + ":" + ImageTag,
                    Name = TestContainerName,
                    Tty = false,
                    HostConfig = hostConfig
                }).GetAwaiter().GetResult();

                this.containerId = response.ID;

                Console.WriteLine("Starting container...");

                var started = client.Containers.StartContainerAsync(this.containerId, new ContainerStartParameters()).GetAwaiter().GetResult();
                if (!started)
                {
                    Assert.False(true, "Cannot start the docker container");
                }

                Console.WriteLine("Finish booting sequence container...");
            }
            catch (DockerApiException e) when (e.StatusCode == HttpStatusCode.Conflict)
            {
                var container = containers.FirstOrDefault(c => c.Names.Contains("/" + TestContainerName));
                if (container is { } c && c.State.Equals("exited", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Starting existing container.");
                    _ = client.Containers.StartContainerAsync(container.ID, new ContainerStartParameters()).GetAwaiter().GetResult();
                }
                else
                {
                    Console.WriteLine("Docker container is already running.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        [OneTimeTearDown]
        protected void TearDown()
        {
            if (!string.IsNullOrEmpty(this.containerId))
            {
                // we are running locally
                var dockerConnection = System.Environment.OSVersion.Platform.ToString().Contains("Win", StringComparison.Ordinal) ?
                    "npipe://./pipe/docker_engine" :
                    "unix:///var/run/docker.sock";
                using var conf = new DockerClientConfiguration(new Uri(dockerConnection)); // localhost
                using var client = conf.CreateClient();
                client.Containers.RemoveContainerAsync(this.containerId, new ContainerRemoveParameters()
                {
                    Force = true
                }).GetAwaiter().GetResult();
            }
        }
    }
}
