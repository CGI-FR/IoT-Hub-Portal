// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    using System.Net;
    using Microsoft.Azure.Devices.Provisioning.Service;

    internal sealed class MockProvisioningServiceClientHttpException : ProvisioningServiceClientHttpException
    {
        public new HttpStatusCode StatusCode { get; set; }

        public MockProvisioningServiceClientHttpException(string message) : base(message)
        {
        }

        public MockProvisioningServiceClientHttpException(string message, System.Exception innerException) : base(message, innerException)
        {
        }

        public MockProvisioningServiceClientHttpException()
        {
        }
    }
}
