using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace RealCarNames
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;

        // Everything Works So Far
        static int ewsfCount;

        // Called by the mod manager
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ewsfCount = 0;
            Logger = modEntry.Logger;

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;

            CarNameProvider.Init();
            //SetCarNames(true); // enabled by default

            return true;
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool state)
        {
            SetCarNames(state);
            return true;
        }

        static void SetCarNames(bool realNames)
        {
            List<Text> displays = new List<Text>(Object.FindObjectsOfType<Text>());

            CarManager.AllCarsList.ForEach(car =>
            {
                Text currentDisplay = displays.Find(item => item.text.Contains(car.name)); // just in case
                car.name = realNames ? CarNameProvider.GetRealName(car.name) : CarNameProvider.GetGameName(car.name);

                // display refresh
                if (currentDisplay != null)
                    currentDisplay.text = car.name;
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
}