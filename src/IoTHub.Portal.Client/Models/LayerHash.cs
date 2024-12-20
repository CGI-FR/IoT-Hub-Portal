// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Models
{
    public class LayerHash
    {
        public LayerDto LayerData { get; set; }
        public HashSet<LayerHash> Children { get; set; } = new HashSet<LayerHash>();
        public int Level { get; set; }
        public bool IsExpanded { get; set; }

        public LayerHash(LayerDto layerData, int level = 0, bool isExpanded = true)
        {
            LayerData = layerData;
            Level = level;
            IsExpanded = isExpanded;
        }
    }
}
