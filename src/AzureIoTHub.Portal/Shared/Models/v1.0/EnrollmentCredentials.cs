using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Shared.Models.V10
{
    public class EnrollmentCredentials
    {
        public string RegistrationID { get; set; }

        public string SymmetricKey { get; set; }

        public string ScopeID { get; set; }

        public string ProvisioningEndpoint { get; set; }
    }
}
