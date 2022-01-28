// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Server.Helpers
{
    using System;
    using AzureIoTHub.Portal.Shared.Models;
    using Newtonsoft.Json.Serialization;

    public class DeviceModelBinder : ISerializationBinder
    {
        // To maintain backwards compatibility with serialized data before using an ISerializationBinder.
        private static readonly DefaultSerializationBinder Binder = new DefaultSerializationBinder();

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            Binder.BindToName(serializedType, out assemblyName, out typeName);
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            // If the type isn't expected, then stop deserialization.
            if (typeName != nameof(DeviceModel))
            {
                return null;
            }

            return Binder.BindToType(assemblyName, typeName);
        }
    }
}
