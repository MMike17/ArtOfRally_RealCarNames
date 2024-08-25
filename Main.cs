using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using UnityModManagerNet;

using Object = UnityEngine.Object;

namespace RealCarNames
{
    public class Main
    {
        public static bool enabled { get; private set; }

        public static UnityModManager.ModEntry.ModLogger Logger;

        // Everything Works So Far
        static int ewsfCount;

        // Called by the mod manager
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ewsfCount = 0;
            Logger = modEntry.Logger;

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;

            CarNameProvider.Init();

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool state)
        {
            enabled = state;
            SetCarNames(state);
            return true;
        }

        static void SetCarNames(bool realNames)
        {
            List<Text> displays = new List<Text>(Object.FindObjectsOfType<Text>());

            CarManager.AllCarsList.ForEach(car =>
            {
                string original = car.name;

                List<Text> currentDisplays = displays.FindAll(display => display.text.Contains(car.name));
                car.name = CarNameProvider.SwitchName(car.name, realNames);

                // displays refresh
                currentDisplays.ForEach(display => display.text = display.text.Replace(original, car.name));
            });

            Log("Setting names to " + (realNames ? "real" : "original") + " variants.");
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
            if (!Main.enabled)
                return;

            __result = CarNameProvider.ReplaceName(__result);
        }
    }
}