// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.Infrastructure.ServicesHealthCheck
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AzureIoTHub.Portal.Infrastructure.ServicesHealthCheck;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class HealthCheckResponseWriterTests
    {
        private MockRepository mockRepository;

        [SetUp]
        public void SetUp()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [Test]
        public async Task WriteHealthReportTest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            using var responseStream = new MemoryStream();

            context.Response.Body = responseStream;

            var entries = new Dictionary<string, HealthReportEntry>()
            {
                { "testOk", new HealthReportEntry(HealthStatus.Healthy, string.Empty, TimeSpan.FromSeconds(1), null, null) },
                { "testKo", new HealthReportEntry(HealthStatus.Unhealthy, "Failed to get result", TimeSpan.FromSeconds(1), null, null) }
            };

            var healthReport = new HealthReport(entries, TimeSpan.FromSeconds(2));

            // Act
            await HealthCheckResponseWriter.WriteHealthReport(
                context,
                healthReport);

            // Assert
            responseStream.Position = 0;
            _ = responseStream.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(responseStream);
            var responseText = await reader.ReadToEndAsync();

            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.AreEqual(/*lang=json,strict*/ "{\"status\":\"Unhealthy\",\"results\":{\"testOk\":{\"status\":\"Healthy\",\"description\":\"\"},\"testKo\":{\"status\":\"Unhealthy\",\"description\":\"Failed to get result\"}}}", responseText);
            this.mockRepository.VerifyAll();
        }
    }
}
