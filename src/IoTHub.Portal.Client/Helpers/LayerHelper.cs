// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Helpers
{
    using System;
    using IoTHub.Portal.Client.Models;
    using IoTHub.Portal.Shared.Models.v10;

    public class LayerHelper
    {

        public static HashSet<LayerHash> GetHashsetLayer(List<LayerDto> listLayers)
        {
            HashSet<LayerHash> Layers = new HashSet<LayerHash> { };
            if (listLayers is null) throw new ArgumentNullException(nameof(listLayers));

            foreach (var layer in listLayers)
            {
                if (layer.Father == null)
                {
                    _ = Layers.Add(new LayerHash(layer));
                    _ = listLayers.Remove(layer);
                    listLayers = AddLayers(Layers, listLayers, Layers.First()).ToList();
                    break;
                }
            }

            return Layers;
        }

        private static List<LayerDto> AddLayers(HashSet<LayerHash> Layers, List<LayerDto> layers, LayerHash currentLayer)
        {
            var listLayers = layers.ToList();
            foreach (var layer in listLayers)
            {
                if (layer.Father == currentLayer.LayerData.Id)
                {
                    _ = currentLayer.Children.Add(new LayerHash(layer, currentLayer.Level + 1, false));
                    _ = layers.Remove(layer);
                    listLayers = AddLayers(Layers, layers, currentLayer.Children.Last()).ToList();
                }
            }
            return listLayers;
        }
    }
}
