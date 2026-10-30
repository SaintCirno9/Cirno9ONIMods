using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Harmony;
using UnityEngine;

namespace ColonyTweaks
{
    public class ElementConverterPatch
    {
        private static Dictionary<string, Dictionary<SimHashes, float>> enhancedBuildings = new()
        {
            {
                "AirFilter", new Dictionary<SimHashes, float>
                {
                    {SimHashes.Clay, 1f},
                }
            },
            {
                "Electrolyzer", new Dictionary<SimHashes, float>
                {
                    {SimHashes.Hydrogen, 0.5f}
                }
            },
            {
                "Compost", new Dictionary<SimHashes, float>
                {
                    {SimHashes.Dirt, 0.5f},
                }
            }
        };

        public static void BatchUpdateElementConverters(string buildingDefPrefabID, GameObject go)
        {
            foreach (var converter in go.GetComponents<ElementConverter>())
            {
                if (converter.consumedElements is not null)
                {
                    var consumedElements = converter.consumedElements;
                    for (int i = 0; i < consumedElements.Length; i++)
                    {
                        consumedElements[i].MassConsumptionRate *= 2f;
                    }

                    foreach (var consumer in go.GetComponents<ConduitConsumer>())
                    {
                        consumer.consumptionRate *= 2f;
                    }

                    foreach (var consumer in go.GetComponents<ElementConsumer>())
                    {
                        consumer.consumptionRate *= 2f;
                    }
                }

                if (converter.outputElements is not null)
                {
                    var outputElements = converter.outputElements;
                    for (int i = 0; i < outputElements.Length; i++)
                    {
                        outputElements[i].massGenerationRate *= 2f;
                        if (enhancedBuildings.ContainsKey(buildingDefPrefabID))
                        {
                            EnhanceOutput(buildingDefPrefabID, ref outputElements[i]);
                        }
                    }
                }
            }
        }

        private static void EnhanceOutput(string buildingDefPrefabID,
            ref ElementConverter.OutputElement outputElement)
        {
            if (enhancedBuildings[buildingDefPrefabID].ContainsKey(outputElement.elementHash))
            {
                outputElement.massGenerationRate += enhancedBuildings[buildingDefPrefabID][outputElement.elementHash];
            }
        }
    }
}