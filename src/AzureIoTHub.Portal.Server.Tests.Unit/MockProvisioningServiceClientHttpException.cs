using Microsoft.Azure.Devices.Provisioning.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit
{
    internal class MockProvisioningServiceClientHttpException : ProvisioningServiceClientHttpException
    {
        public new HttpStatusCode StatusCode { get; set; }
    }
}
