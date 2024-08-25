using System;
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

        static List<string> gameNames;
        static List<string> realNames;

        // called by Main
        public static void Init()
        {
            gameNames = GetNamesFromFile(GAME_NAMES_FILE);
            realNames = GetNamesFromFile(REAL_NAMES_FILE);

            if (gameNames.Count != realNames.Count)
                Main.Log("Game names and real names list do not match, please check the lists of names for missing or duplicates.");
            else
                Main.Log("Loaded " + gameNames.Count + " car names");
        }

        static List<string> GetNamesFromFile(string fileName)
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

        public static string GetRealName(string carName)
        {
            int index = gameNames.IndexOf(carName);

            if (index != -1)
                return realNames[index];
            else if (realNames.Contains(carName))
                Main.Log("Name is already set to real variant.");
            else
                Main.Log("Couldn't find \"" + carName + "\" in the provided lists of names. Check the lists of car names.");

            return carName;
        }

        public static string GetGameName(string carName)
        {
            int index = realNames.IndexOf(carName);

            if (index != -1)
                return gameNames[index];
            else if (gameNames.Contains(carName))
                Main.Log("Name is already set to game variant.");
            else
                Main.Log("Couldn't find \"" + carName + "\" in the provided lists of names. Check the lists of car names.");

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

            return description.Replace(gameNames[carNameIndex], realNames[carNameIndex]);
        }
    }
}
