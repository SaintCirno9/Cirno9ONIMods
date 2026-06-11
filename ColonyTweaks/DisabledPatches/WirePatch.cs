using System.Collections.Generic;
using Harmony;

namespace ColonyTweaks
{
    public class WirePatch
    {
        [HarmonyPatch(typeof(Wire), "GetMaxWattageAsFloat")]
        public class WireGetMaxWattageAsFloatPatch
        {
            private static bool Prefix(Wire.WattageRating rating, ref float __result)
            {
                float num;
                switch (rating)
                {
                    case Wire.WattageRating.Max500:
                        num = ColonyTweaks.WireWattageRating[0];
                        break;
                    case Wire.WattageRating.Max1000:
                        num = ColonyTweaks.WireWattageRating[1];
                        break;
                    case Wire.WattageRating.Max2000:
                        num = ColonyTweaks.WireWattageRating[2];
                        break;
                    case Wire.WattageRating.Max20000:
                        num = ColonyTweaks.WireWattageRating[3];
                        break;
                    case Wire.WattageRating.Max50000:
                        num = ColonyTweaks.WireWattageRating[4];
                        break;
                    default:
                        num = 0f;
                        break;
                }

                __result = num;
                return false;
            }
        }
    }
}