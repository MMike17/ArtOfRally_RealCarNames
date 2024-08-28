using HarmonyLib;
using UnityEngine;

namespace RealCarNames
{
    [HarmonyPatch(typeof(Car.CarStats), nameof(Car.CarStats.GetLoreStringLocalized))]
    static class CarStats_GetLoreStringLocalized_Patch
    {
        static void Postfix(ref string __result)
        {
            if (Main.settings == null || !Main.enabled || Main.settings.nameFormat == Settings.Format.original)
                return;

            __result = CarNameProvider.ReplaceName(__result);
            Object.FindObjectOfType<CarChooserHelper>().CarButton.CarHistoryText.supportRichText = true;
        }
    }
}
