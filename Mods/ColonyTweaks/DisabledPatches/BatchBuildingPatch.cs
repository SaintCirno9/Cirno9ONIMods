using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using Harmony;
using STRINGS;
using TUNING;

namespace ColonyTweaks
{
    public class BatchPatch
    {
        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public class GeneratedBuildingsLoadGeneratedBuildingsPatch
        {
            public static void Postfix()
            {
                foreach (var buildingDef in Assets.BuildingDefs)
                {
                    if (buildingDef is not {BuildingComplete: { }})
                    {
                        continue;
                    }

                    var complete = buildingDef.BuildingComplete;
                    var buildingDefPrefabID = buildingDef.PrefabID;
                    StoragePatch.BatchUpdateStorages(buildingDefPrefabID, complete);
                    ElementConverterPatch.BatchUpdateElementConverters(buildingDefPrefabID, complete);
                    ElementConsumerPatch.BatchUpdateElementConsumers(buildingDefPrefabID, complete);
                    GeneratorPatch.BatchUpdateGenerator(buildingDefPrefabID, buildingDef, complete);
                    SimCellOccupierPatch.BatchAddSimCellOccupierControl(buildingDefPrefabID, complete);
                    TilePatch.BatchUpdateTileProperties(buildingDefPrefabID, complete);
                }
            }
        }

        [HarmonyPatch(typeof(BuildingConfigManager), "RegisterBuilding")]
        public class BuildingConfigManagerRegisterBuildingPatch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    var opString = codes[i].opcode + " " + codes[i].operand;
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i + 1].opcode == OpCodes.Stloc_0)
                    {
                        codes.InsertRange(i + 1, new[]
                        {
                            new CodeInstruction(OpCodes.Callvirt,
                                typeof(BuildingConfigManagerRegisterBuildingPatch).GetMethod(
                                    nameof(ModifyBuildingDef)))
                        });
                        break;
                    }
                }

                return codes.AsEnumerable();
            }

            public static BuildingDef ModifyBuildingDef(BuildingDef def)
            {
                def.Floodable = false;
                def.OverheatTemperature += 50f;
                if (def.PrefabID is "SolidTransferArm" or "AutoMiner" or "SolidConduitInbox")
                {
                    def.SelfHeatKilowattsWhenActive = 0f;
                }

                if (def.ThermalConductivity < 1f || def.PrefabID is "CarpetTile")
                {
                    def.ThermalConductivity = 0.01f;
                }

                if (def.ThermalConductivity > 1f ||def.PrefabID is "MetalTile" or "GlassTile")
                {
                    def.ThermalConductivity = 10f;
                }

                if (def.BuildLocationRule is BuildLocationRule.NotInTiles)
                {
                    def.BuildLocationRule = BuildLocationRule.Anywhere;
                }

                return def;
            }
        }
    }
}