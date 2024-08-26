﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RealCarNames
{
    static class CarNameProvider
    {
        // embeded text files with names
        const string GAME_NAMES_FILE = "RealCarNames.GameNames.txt";
        const string REAL_NAMES_FILE = "RealCarNames.RealNames.txt";
        const string YEARS_FILE = "RealCarNames.Years.txt";

        static List<string> gameNames;
        static List<string> realNames;
        static List<string> years;

        // called by Main
        public static void Init()
        {
            gameNames = GetLinesFromFile(GAME_NAMES_FILE);
            realNames = GetLinesFromFile(REAL_NAMES_FILE);
            years = GetLinesFromFile(YEARS_FILE);

            if (gameNames.Count != realNames.Count)
                Main.Log("Game names and real names list do not match, please check the lists of names for missing or duplicates.");
            else
                Main.Log("Loaded " + gameNames.Count + " car names");
        }

        static List<string> GetLinesFromFile(string fileName)
        {
            List<string> gameNames = new List<string>();

            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName))
            {
                if (stream == null)
                {
                    Main.Log("Couldn't read local files. Make sure the files have been included in the build.");
                    return null;
                }

                try
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                            gameNames.Add(reader.ReadLine());
                    }
                }
                catch (Exception e)
                {
                    Main.Log("Error while reading local files. " + e.ToString());
                }
            }

            return gameNames;
        }

        public static string SwitchName(string carName)
        {
            // remove dates in name if found
            if (carName.Contains("("))
                carName = carName.Split(new string[] { " (" }, StringSplitOptions.None)[0];

            List<string> source = gameNames.Contains(carName) ? gameNames : realNames.Contains(carName) ? realNames : null;

            if (source == null)
            {
                Main.Log("Couldn't find \"" + carName + "\" in the provided lists of names. Check the lists of car names.");
                return carName;
            }

            List<string> target = Main.settings.realNames ? realNames : gameNames;
            int carIndex = source.IndexOf(carName);

            if (source != target)
                carName = target[carIndex];

            if (Main.settings.withDates)
                carName += " " + years[carIndex];

            return carName;
        }

        public static string ReplaceName(string description)
        {
            int carNameIndex = -1;

            for (int i = 0; i < gameNames.Count; i++)
            {
                if (description.Contains(gameNames[i]))
                {
                    if (carNameIndex != -1)
                    {
                        Main.Log("Found multiple car names in the same description. Keeping first name.");
                        break;
                    }

                    carNameIndex = i;
                }
            }

            if (carNameIndex == -1)
            {
                Main.Log("Couldn't find a car name in the description, skipping substitution.");
                return description;
            }

            return description.Replace(gameNames[carNameIndex], SwitchName(gameNames[carNameIndex]));
        }
    }
}
