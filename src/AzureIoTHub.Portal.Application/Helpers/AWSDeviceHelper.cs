// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Application.Helpers
{
    using System;
    using Amazon.IotData.Model;
    using Newtonsoft.Json.Linq;

    public static class AWSDeviceHelper
    {
        public static JObject RetrieveDesiredProperties(GetThingShadowResponse shadow)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var jsonObject = JObject.Parse(payloadString);

            var stateNode = jsonObject["state"] as JObject;
            var desiredNode = stateNode!["desired"] as JObject;

            return desiredNode!;
        }

        public static JObject RetrieveReportedProperties(GetThingShadowResponse shadow)
        {
            ArgumentNullException.ThrowIfNull(shadow, nameof(shadow));

            var payloadString = System.Text.Encoding.UTF8.GetString(shadow.Payload.ToArray());
            var jsonObject = JObject.Parse(payloadString);

            var stateNode = jsonObject["state"] as JObject;
            var reportedNode = stateNode!["reported"] as JObject;

            return reportedNode!;
        }
    }
}
