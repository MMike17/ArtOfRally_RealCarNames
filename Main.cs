using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace RealCarNames
{
    public class Main
    {
        public static bool enabled { get; private set; }

        public static UnityModManager.ModEntry.ModLogger Logger;
        public static Settings settings;

        // "Everything Works So Far"
        static int ewsfCount;

        // Called by the mod manager
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ewsfCount = 0;
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = entry => settings.Draw(modEntry);
            modEntry.OnSaveGUI = entry => settings.Save(modEntry);

            CarNameProvider.Init();

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool state)
        {
            enabled = state;
            RefreshCarNames();
            return true;
        }

        public static void RefreshCarNames()
        {
            List<Text> displays = new List<Text>(Resources.FindObjectsOfTypeAll<Text>());

            CarManager.AllCarsList.ForEach(car =>
            {
                string original = car.name;

                List<Text> currentDisplays = displays.FindAll(display => display.text.Contains(car.name));
                car.name = CarNameProvider.SwitchName(car.name, settings.nameFormat);

                // displays refresh if needed
                if (original != car.name)
                    currentDisplays.ForEach(display => display.text = display.text.Replace(original, car.name));
            });
        }

        public static void LogEwSF()
        {
            ewsfCount++;
            Log("(" + ewsfCount + ") EwSF");
        }

        public static void Log(string message) => Logger.Log(message);
    }

    [HarmonyPatch(typeof(Car.CarStats), nameof(Car.CarStats.GetLoreStringLocalized))]
    static class CarStats_GetLoreStringLocalized_Patch
    {
        static void Postfix(ref string __result)
        {
            if (Main.settings == null || !Main.enabled || Main.settings.nameFormat == Settings.Format.original)
                return;

            __result = CarNameProvider.ReplaceName(__result);
        }
    }
}