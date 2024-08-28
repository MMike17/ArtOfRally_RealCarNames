using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using static UnityEngine.RectTransform;

namespace RealCarNames.Patches
{
    [HarmonyPatch(typeof(LeaderboardScreenUpdater), "UpdateLeaderboardUI")]
    static class LeaderboardScreenUpdater_UpdateLeaderboardUI_Patch
    {
        static float originalNameSize;
        static float originalCarSize;
        static List<LeaderboardEntry> list;

        public static void RefreshLeaderboard()
        {
            if (list == null)
                return;

            Postfix(null);
        }

        static void Postfix(LeaderboardScreenUpdater __instance)
        {
            if (list == null)
            {
                FieldInfo entryList = typeof(LeaderboardScreenUpdater).GetField(
                    "LeaderboardEntriesList",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                list = entryList.GetValue(__instance) as List<LeaderboardEntry>;
            }

            if (list == null)
            {
                Main.Log("Couldn't retrieve leaderboard entry list.");
                return;
            }

            if (list.Count == 0)
            {
                Main.Log("List is empty, skipping.");
                return;
            }

            if (originalNameSize == 0)
                originalNameSize = list[0].Name.rectTransform.sizeDelta.x;

            if (originalCarSize == 0)
                originalCarSize = list[0].Car.rectTransform.sizeDelta.x;

            float targetNameSize = originalNameSize;
            float targetCarSize = originalCarSize;

            if (Main.enabled)
            {
                float spacing = Main.settings.extraLeaderboardSpacing + list[0].Name.GetComponentInParent<HorizontalLayoutGroup>().spacing;
                float maxNameWidth = 0;
                float maxCarWidth = 0;

                list.ForEach(entry =>
                {
                    float currentNameWidth = LayoutUtility.GetPreferredWidth(entry.Name.rectTransform);
                    float currentCarWidth = LayoutUtility.GetPreferredWidth(entry.Car.rectTransform);

                    maxNameWidth = Mathf.Max(maxNameWidth, currentNameWidth);
                    maxCarWidth = Mathf.Max(maxCarWidth, currentCarWidth);
                });

                targetNameSize = maxNameWidth + spacing;
                targetCarSize = maxCarWidth + spacing;
            }

            list.ForEach(entry =>
            {
                entry.Name.rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, targetNameSize);
                entry.Car.rectTransform.SetSizeWithCurrentAnchors(Axis.Horizontal, targetCarSize);
            });

            // this is not optimal at all
            LayoutRebuilder.MarkLayoutForRebuild(list[0].Name.rectTransform.parent.GetComponent<RectTransform>());
        }
    }
}
