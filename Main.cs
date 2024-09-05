using HarmonyLib;
using System;
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

        // Called by the mod manager
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            Logger = modEntry.Logger;
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            Harmony harmony = new Harmony(modEntry.Info.Id);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // make sure this is called before OnToggle
            CarNameProvider.Init();

            // hook in mod manager event
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = entry => settings.Draw(modEntry);
            modEntry.OnSaveGUI = entry => settings.Save(modEntry);

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
                car.name = CarNameProvider.SwitchName(car, enabled ? settings.nameFormat : Settings.Format.original);

                // displays refresh if needed
                if (original != car.name)
                    currentDisplays.ForEach(display => display.text = display.text.Replace(original, car.name));
            });

            Log("Refreshed car names to format " + (enabled ? settings.nameFormat : Settings.Format.original));
        }

        public static void Log(string message) => Logger.Log(message);

        public static void Try(Action callback)
        {
            try
            {
                callback?.Invoke();
            }
            catch (Exception e)
            {
                Log(e.ToString());
            }
        }
    }
}