﻿using UnityModManagerNet;
using UnityEngine.SceneManagement;

namespace RealCarNames
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;

        public static bool enabled { get; private set; }

        static int ewsfCount;

        // Called by the mod manager
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ewsfCount = 0;
            Logger = modEntry.Logger;

            HookEvents(modEntry);

            CarNameProvider.Init();

            return true;
        }

        static void HookEvents(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;

            // scout scene names
            SceneManager.sceneLoaded += (scene, mode) => Log("Changed scene : " + scene.name + " / " + scene.buildIndex);
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool state)
        {
            enabled = state;
            return true;
        }

        public static void LogEwSF()
        {
            ewsfCount++;
            Log("(" + ewsfCount + ") EwSF");
        }

        public static void Log(string message) => Logger.Log(message);
    }
}