using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using static RealCarNames.Settings;
using static UnityEngine.RectTransform;

namespace RealCarNames.Patches
{
    // replaces car name in car description
    [HarmonyPatch(typeof(Car.CarStats), nameof(Car.CarStats.GetLoreStringLocalized))]
    static class CarStats_GetLoreStringLocalized_Patch
    {
        static void Postfix(ref string __result)
        {
            if (Main.settings == null || !Main.enabled || Main.settings.nameFormat == Format.original)
                return;

            string result = __result;

            Main.Try(() =>
            {
                result = CarNameProvider.ReplaceName(result);
                CarChooserHelper helper = GameObject.FindObjectOfType<CarChooserHelper>();

                if (helper.CarButton.CarHistoryText != null)
                    helper.CarButton.CarHistoryText.supportRichText = true;
            });

            __result = result;
        }
    }

    // fixes spacing in leaderboards
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
            Main.Try(() =>
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
            });
        }
    }

    // fixes spacing in stage results leaderboards
    [HarmonyPatch(typeof(StageResults), nameof(StageResults.UpdateEventResults))]
    static class StageResults_UpdateEventResults_Patch
    {
        static void Postfix(StageResults __instance)
        {
            StageResults_UpdateStageResults_Patch.Postfix(__instance);
        }
    }

    // fixes spacing in event results leaderboards
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
            Main.Try(() =>
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
            });
        }
    }

    // fixes spacing in end of season leaderboards
    [HarmonyPatch(typeof(SeasonStandingsScreen))]
    static class SeasonStandingsScreen_Init_Patch
    {
        static List<CustomEntry> list;

        public static void RefreshLeaderboard()
        {
            if (list == null)
                return;

            InitPostfix(null);
        }

        [HarmonyPatch(nameof(SeasonStandingsScreen.Refresh))]
        [HarmonyPostfix]
        static void InitPostfix(SeasonStandingsScreen __instance)
        {
            if (!Main.enabled)
                return;

            if (__instance == null && list == null)
                return;

            Main.Try(() =>
            {
                list = new List<CustomEntry>();
                Transform root = __instance.transform.GetChild(0);
                float sideSpacing = root.position.x / 6;

                root = __instance.transform.GetChild(1);
                root.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                root.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
                root.position = new Vector3(root.position.x + sideSpacing, root.position.y, root.position.z);

                if (root.childCount == 0)
                {
                    Main.Log("Don't have list");
                    return;
                }

                for (int i = 0; i < RallyData.NUM_AI_DRIVERS; i++)
                    list.Add(new CustomEntry(root.GetChild(i)));

                __instance.StartCoroutine(UpdateWhenReady(
                    () => Main.Try(() =>
                    {
                        float[] widths = new float[4];

                        list.ForEach(item =>
                        {
                            widths[0] = Mathf.Max(widths[0], item.GetPreferedWidth(0));
                            widths[1] = Mathf.Max(widths[1], item.GetPreferedWidth(2));
                            widths[2] = Mathf.Max(widths[2], item.GetPreferedWidth(3));
                            widths[3] = Mathf.Max(widths[3], item.GetPreferedWidth(4));
                        });

                        list.ForEach(item => item.FitNameAndCar(widths));
                    })
                ));
            });
        }

        public static IEnumerator UpdateWhenReady(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }
    }

    // thanks devs for making your class internal....now I have to do the work twice...
    // At least I get to make it clean
    class CustomEntry
    {
        Transform root;
        HorizontalLayoutGroup group;

        public CustomEntry(Transform root)
        {
            this.root = root;

            group = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.childForceExpandHeight = group.childForceExpandWidth = true;
            group.childControlWidth = group.childControlHeight = false;
            group.spacing = Main.settings.extraLeaderboardSpacing;

            root.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            foreach (Transform child in root)
                child.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        public float GetPreferedWidth(int index) => LayoutUtility.GetPreferredWidth(root.GetChild(index).GetComponent<RectTransform>());

        public void FitNameAndCar(float[] sizes)
        {
            root.GetChild(0).gameObject.AddComponent<LayoutElement>().minWidth = sizes[0];
            root.GetChild(2).gameObject.AddComponent<LayoutElement>().minWidth = sizes[1];
            root.GetChild(3).gameObject.AddComponent<LayoutElement>().minWidth = sizes[2];
            root.GetChild(4).gameObject.AddComponent<LayoutElement>().minWidth = sizes[3];
        }
    }
}