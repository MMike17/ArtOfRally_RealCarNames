using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using static UnityEngine.RectTransform;

namespace RealCarNames.Patches
{
    [HarmonyPatch(typeof(Car.CarStats), nameof(Car.CarStats.GetLoreStringLocalized))]
    static class CarStats_GetLoreStringLocalized_Patch
    {
        static void Postfix(ref string __result)
        {
            if (Main.settings == null || !Main.enabled || Main.settings.nameFormat == Settings.Format.original)
                return;

            __result = CarNameProvider.ReplaceName(__result);
            GameObject.FindObjectOfType<CarChooserHelper>().CarButton.CarHistoryText.supportRichText = true;
        }
    }

    [HarmonyPatch(typeof(LeaderboardScreenUpdater), "UpdateLeaderboardUI")]
    static class LeaderboardScreenUpdater_UpdateLeaderboardUI_Patch
    {
        static float originalNameSize;
        static float originalCarSize;
        static List<LeaderboardEntry> list;

        static bool IsListValid()
        {
            if (list == null)
                return false;

            if (list.Count == 0)
                return false;

            if (list[0] == null || list[0].Name == null)
                return false;

            return true;
        }

        public static void RefreshLeaderboard()
        {
            if (list == null)
                return;

            Postfix(null);
        }

        static void Postfix(LeaderboardScreenUpdater __instance)
        {
            if (__instance != null)
            {
                FieldInfo entryList = typeof(LeaderboardScreenUpdater).GetField(
                    "LeaderboardEntriesList",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
                list = entryList.GetValue(__instance) as List<LeaderboardEntry>;
            }

            if (!IsListValid())
            {
                Main.Log("Couldn't retrieve leaderboard entry list. Aborting");
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

    [HarmonyPatch(typeof(StageResults), nameof(StageResults.UpdateEventResults))]
    static class StageResults_UpdateEventResults_Patch
    {
        static void Postfix(StageResults __instance) => StageResults_UpdateStageResults_Patch.Postfix(__instance);
    }

    [HarmonyPatch(typeof(StageResults), nameof(StageResults.UpdateStageResults))]
    static class StageResults_UpdateStageResults_Patch
    {
        static float originalNameSize;
        static float originalCarSize;
        static List<StageEntry> list;

        static bool IsListValid()
        {
            if (list == null)
                return false;

            if (list.Count == 0)
                return false;

            if (list[0] == null)
                return false;

            return true;
        }

        public static void RefreshStageResults()
        {
            if (list == null)
                return;

            Postfix(null);
        }

        public static void Postfix(StageResults __instance)
        {
            if (__instance != null)
            {
                FieldInfo entryList = typeof(StageResults).GetField("StandingsList", BindingFlags.NonPublic | BindingFlags.Instance);
                list = entryList.GetValue(__instance) as List<StageEntry>;
            }

            if (!IsListValid())
            {
                Main.Log("Couldn't retrive stage result entry list. Aborting.");
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

    [HarmonyPatch(typeof(SeasonStandingsScreen), nameof(SeasonStandingsScreen.Init))]
    static class SeasonStandingsScreen_Init_Patch
    {
        static List<CustomEntry> list;

        public static void RefreshLeaderboard()
        {
            if (list == null)
                return;

            Postfix(null);
        }

        static void Postfix(SeasonStandingsScreen __instance)
        {
            Main.Log("Started end screen postfix (has instance : " + (__instance != null) + ")");

            __instance.StartCoroutine(UpdateWhenReady(__instance.GetComponent<CanvasGroup>(), () =>
            {
                if (__instance != null)
                {
                    list = new List<CustomEntry>();
                    Transform root = __instance.transform.GetChild(1);

                    for (int i = 0; i < RallyData.NUM_AI_DRIVERS; i++)
                        list.Add(new CustomEntry(root.GetChild(i)));

                    Main.Log("Refreshed standing refs");
                }

                if (list == null || list.Count == 0)
                {
                    Main.Log("Don't have list");
                    return;
                }

                if (Main.enabled)
                {
                    Main.Log("Setting sizes");

                    float maxNameWidth = 0;
                    float maxCarWidth = 0;

                    list.ForEach(entry =>
                    {
                        maxNameWidth = Mathf.Max(maxNameWidth, entry.GetPreferedNameWidth());
                        maxCarWidth = Mathf.Max(maxCarWidth, entry.GetPreferedCarWidth());
                    });

                    Main.Log("Get max sizes");

                    list.ForEach(entry => entry.FitNameAndCar(maxNameWidth, maxCarWidth));
                }
                else
                {
                    Main.Log("Reset");
                    list.ForEach(entry => entry.FitNameAndCar(entry.originalNameSize, entry.originalCarSize));
                }
            }));
        }

        static IEnumerator UpdateWhenReady(CanvasGroup obj, Action callback)
        {
            yield return new WaitUntil(() => obj.alpha > 0);
            callback?.Invoke();
        }

        // thanks devs for making your class internal....now I have to do the work twice...
        // At least I get to make it clean
        class CustomEntry
        {
            const float BASE_SPACING = 12;

            public float originalNameSize;
            public float originalCarSize;

            Transform root;

            public CustomEntry(Transform root)
            {
                this.root = root;

                originalNameSize = root.GetChild(2).GetComponent<RectTransform>().sizeDelta.x;
                originalCarSize = root.GetChild(3).GetComponent<RectTransform>().sizeDelta.x;
            }

            public float GetPreferedNameWidth() => LayoutUtility.GetPreferredWidth(root.GetChild(2).GetComponent<RectTransform>());
            public float GetPreferedCarWidth() => LayoutUtility.GetPreferredWidth(root.GetChild(3).GetComponent<RectTransform>());

            public void FitNameAndCar(float nameSize, float carSize)
            {
                RectTransform startRect = root.GetChild(1).GetComponent<RectTransform>();
                Vector3 startPos = startRect.position + (startRect.sizeDelta.x / 2 + BASE_SPACING) * Vector3.right;
                float spacing = BASE_SPACING + Main.settings.extraLeaderboardSpacing;

                // start at Name (index 2)
                for (int i = 2; i < root.childCount; i++)
                {
                    RectTransform current = root.GetChild(i).GetComponent<RectTransform>();

                    // adjust size of 2 and 3 (Name and Car)
                    if (i < 4)
                        current.SetSizeWithCurrentAnchors(Axis.Horizontal, i == 2 ? nameSize : carSize);

                    current.position = startPos + (current.sizeDelta.x / 2) * Vector3.right;
                    startPos += (spacing + current.sizeDelta.x) * Vector3.right;
                }
            }
        }
    }
}