// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Infrastructure.Helpers
{
    using System.Reflection;

    public class EdgeEnrollementHelper : IEdgeEnrollementHelper
    {
        public string? GetEdgeEnrollementTemplate(string templateName)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            using var resourceStream = currentAssembly.GetManifestResourceStream($"{currentAssembly.GetName().Name}.EdgeEnrollementScripts.{templateName}");

            if (resourceStream == null)
                return null;

            using var streamReader = new StreamReader(resourceStream);

            return streamReader.ReadToEnd();
        }
    }
}
