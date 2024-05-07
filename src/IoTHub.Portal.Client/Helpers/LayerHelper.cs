// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Helpers
{
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Shared.Models.v10;

    public class LayerHelper
    {
        public HashSet<LayerHash> Layers { get; set; } = new HashSet<LayerHash>();

        public static HashSet<LayerHash> GetHashsetLayer(List<LayerDto> listLayers)
        {
            HashSet<LayerHash> Layers = new HashSet<LayerHash>();
            OrderLayers(Layers, listLayers);
            return Layers;
        }

        private static void OrderLayers(HashSet<LayerHash> Layers, List<LayerDto> layers)
        {
            if (layers.Count() == 0) return;

            LayerDto firstLayer = layers[0];
            bool isAdd;

            if (firstLayer.Father == "Init")
            {
                Layers.Add(new LayerHash(firstLayer, 0, false));
                isAdd = true;
            }
            // Look for the layer's father within Layer
            else
            {
                isAdd = addSubLayer(Layers, firstLayer);
            }

            // If the layer hasn't been added, place it at the end of the list to wait for its father to be added to "Layers"
            layers.RemoveAt(0);
            if (!isAdd) layers.Add(firstLayer);

            OrderLayers(Layers, layers);
        }

        private static bool addSubLayer(HashSet<LayerHash> fatherLayer, LayerDto layer)
        {
            foreach (LayerHash father in fatherLayer)
            {
                if (father.LayerData.Id == layer.Father)
                {
                    father.Children.Add(new LayerHash(layer, father.Level + 1, false));

                    // If father has been found, return true
                    return true;
                }
                else
                {
                    // Look for the layer's father within Layer
                    bool isAdd = addSubLayer(father.Children, layer);

                    // If father has been found, return true
                    if (isAdd) return true;
                }
            }
            return false;
        }
    }
}
