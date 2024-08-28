using HarmonyLib;
using UnityEngine;

namespace RealCarNames.Patches
{
    [HarmonyPatch(typeof(LeaderboardScreenUpdater), nameof(LeaderboardScreenUpdater.OnEnterScreen))]
    static class LeaderboardScreenUpdater_OnEnterScreen_Patch
    {
        static void Postfix(ref string __result)
        {
            if (!Main.enabled)
                return;

            //if (Main.settings == null || !Main.enabled || Main.settings.nameFormat == Settings.Format.original)
            //    return;

            //__result = CarNameProvider.ReplaceName(__result);
            //Object.FindObjectOfType<CarChooserHelper>().CarButton.CarHistoryText.supportRichText = true;
        }
    }
}
