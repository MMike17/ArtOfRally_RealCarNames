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

        public static string SwitchName(string carName, Settings.Format format)
        {
            List<string> source = null;
            int carIndex = -1;

            // have to do in-depth search for composite names
            for (int i = 0; i < gameNames.Count; i++)
            {
                if (carName.Contains(gameNames[i]))
                {
                    source = gameNames;
                    carIndex = i;
                    break;
                }
            }

            if (source == null)
            {
                for (int i = 0; i < realNames.Count; i++)
                {
                    if (carName.Contains(realNames[i]))
                    {
                        source = realNames;
                        carIndex = i;
                        break;
                    }
                }
            }

            if (source == null)
            {
                Main.Log("Couldn't find \"" + carName + "\" in the provided lists of names. Check the lists of car names.");
                return carName;
            }

            // I wish I could use the switch expression...
            switch (format)
            {
                case Settings.Format.original:
                    carName = gameNames[carIndex];
                    break;

                case Settings.Format.real:
                    carName = realNames[carIndex];
                    break;

                case Settings.Format.original_year:
                    carName = BuildName(gameNames[carIndex], years[carIndex]);
                    break;

                case Settings.Format.real_year:
                    carName = BuildName(realNames[carIndex], years[carIndex]);
                    break;

                case Settings.Format.original_year_real:
                    carName = BuildName(gameNames[carIndex], years[carIndex] + " " + realNames[carIndex]);
                    break;

                case Settings.Format.real_original_year:
                    carName = BuildName(realNames[carIndex], "\"" + gameNames[carIndex] + "\" " + years[carIndex]);
                    break;
            }

            return carName;
        }

        static string BuildName(string start, string second)
        {
            string result = start;

            if (Main.settings.lowerSize)
                result += "<size=" + Main.settings.lowTextSize + ">";

            result += " ";

            if (Main.settings.parenthesis)
                result += "(";

            // cleaner
            result += string.IsNullOrEmpty(second) ? string.Empty : second;

            if (Main.settings.parenthesis)
                result += ")";

            if (Main.settings.lowerSize)
                result += "</size>";

            return result;
        }

        public static string ReplaceName(string description)
        {
            List<string> detectedNames = new List<string>();

            for (int i = 0; i < gameNames.Count; i++)
            {
                string checkedName = gameNames[i];

                if (description.Contains(checkedName) && !detectedNames.Contains(checkedName))
                    detectedNames.Add(checkedName);
            }

            if (detectedNames.Count == 0)
            {
                Main.Log("Couldn't find a car name in the description, skipping substitution.");
                return description;
            }

            Settings.Format targetFormat = Main.settings.nameFormat;

            if (targetFormat == Settings.Format.original_year_real)
                targetFormat = Settings.Format.original_year;

            if (targetFormat == Settings.Format.real_original_year)
                targetFormat = Settings.Format.real_year;

            detectedNames.ForEach(name => description = description.Replace(name, SwitchName(name, targetFormat)));
            return description;
        }
    }
}
