// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace IoTHub.Portal.Client.Helpers
{
    public static class LayerHelper
    {
        public static HashSet<LayerHash> GetHashsetLayer(List<LayerDto> listLayers)
        {
            var layers = new HashSet<LayerHash> { };
            if (listLayers is null) throw new ArgumentNullException(nameof(listLayers));

            foreach (var layer in listLayers)
            {
                if (layer.Father == null)
                {
                    _ = layers.Add(new LayerHash(layer));
                    _ = listLayers.Remove(layer);
                    listLayers = AddLayers(layers, listLayers, layers.First()).ToList();
                    break;
                }
            }
            return layers;
        }

        private static List<LayerDto> AddLayers(HashSet<LayerHash> layers, List<LayerDto> layersDto, LayerHash currentLayer)
        {
            var listLayers = layersDto.ToList();
            foreach (var layer in listLayers)
            {
                if (layer.Father == currentLayer.LayerData.Id)
                {
                    _ = currentLayer.Children.Add(new LayerHash(layer, currentLayer.Level + 1, false));
                    _ = layersDto.Remove(layer);
                    listLayers = AddLayers(layers, layersDto, currentLayer.Children.Last()).ToList();
                }
            }
            return listLayers;
        }
    }
}
