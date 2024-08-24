using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RealCarNames
{
    public static class CarNameProvider
    {
        // embeded text files with names
        const string GAME_NAMES_FILE = "RealCarNames.GameNames.txt";
        const string REAL_NAMES_FILE = "RealCarNames.RealNames.txt";

        static Dictionary<string, string> substitutionTable;

        // called by Main
        public static void Init()
        {
            string[] gameNames = GetNamesFromFile(GAME_NAMES_FILE);
            string[] realNames = GetNamesFromFile(REAL_NAMES_FILE);

            substitutionTable = new Dictionary<string, string>();

            for (int i = 0; i < gameNames.Length; i++)
                substitutionTable.Add(gameNames[i], realNames[i]);

            Main.Log("Loaded " + gameNames.Length + " car names");
        }

        static string[] GetNamesFromFile(string fileName)
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

            return gameNames.ToArray();
        }

        public static string GetCarName(string gameName)
        {
            if (!Main.enabled)
                return gameName;

            if (!substitutionTable.ContainsKey(gameName))
            {
                Main.Log("Couldn't find real name for car : " + gameName);
                return gameName;
            }

            return substitutionTable[gameName];
        }
    }
}
