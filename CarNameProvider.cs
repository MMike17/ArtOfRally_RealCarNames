using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RealCarNames
{
    public static class CarNameProvider
    {
        // embeded text files with names
        const string GAME_NAMES_FILE = "RealCarNames.Data.GameNames.txt";
        const string REAL_NAMES_FILE = "RealCarNames.Data.RealNames.txt";
        const string YEARS_FILE = "RealCarNames.Data.Years.txt";

        public static List<string> gameNames;
        public static List<string> years;
        static List<string> realNames;

        // called by Main
        public static void Init()
        {
            gameNames = GetLinesFromFile(GAME_NAMES_FILE);
            realNames = GetLinesFromFile(REAL_NAMES_FILE);
            years = GetLinesFromFile(YEARS_FILE);

            if (gameNames.Count != realNames.Count)
                Main.Log("Game names and real names list do not match, please check the lists of names for missing or duplicates.");
            else
                Main.Log("Loaded " + gameNames.Count + " car data.");
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
            int carIndex = -1;

            // have to do in-depth search for composite names
            for (int i = 0; i < gameNames.Count; i++)
            {
                if (carName.Contains(gameNames[i]))
                {
                    carIndex = i;
                    break;
                }
            }

            if (carIndex == -1)
            {
                for (int i = 0; i < realNames.Count; i++)
                {
                    if (carName.Contains(realNames[i]))
                    {
                        carIndex = i;
                        break;
                    }
                }
            }

            string result = string.Empty;

            // I wish I could use the switch expression...
            switch (format)
            {
                case Settings.Format.original:
                    result = gameNames[carIndex];
                    break;

                case Settings.Format.real:
                    result = realNames[carIndex];
                    break;

                case Settings.Format.original_year:
                    result = BuildName(gameNames[carIndex], years[carIndex]);
                    break;

                case Settings.Format.real_year:
                    result = BuildName(realNames[carIndex], years[carIndex]);
                    break;

                case Settings.Format.original_year_real:
                    result = BuildName(gameNames[carIndex], years[carIndex] + " " + realNames[carIndex]);
                    break;

                case Settings.Format.real_original_year:
                    result = BuildName(realNames[carIndex], "\"" + gameNames[carIndex] + "\" " + years[carIndex]);
                    break;
            }

            //Main.Log(carName + " => " + result);
            return result;
        }

        static string BuildName(string start, string second)
        {
            string result = start;

            if (Main.settings.lowerSize)
                result += "<size=" + Main.settings.lowTextSize + ">";

            result += " ";

            if (Main.settings.parenthesis)
                result += "(";

            result += second;

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

            if (!Main.enabled)
                targetFormat = Settings.Format.original;

            detectedNames.ForEach(name => description = description.Replace(name, SwitchName(name, targetFormat)));
            return description;
        }
    }
}
