using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using static RealCarNames.Settings;

namespace RealCarNames
{
    public static class CarNameProvider
    {
        // embeded text files with names
        const string GAME_NAMES_FILE = "RealCarNames.Data.GameNames.txt";
        const string REAL_NAMES_FILE = "RealCarNames.Data.RealNames.txt";
        const string YEARS_FILE = "RealCarNames.Data.Years.txt";

        public static List<string> years;
        public static List<string> gameNames;
        public static List<string> realNames;

        // called by Main
        public static void Init()
        {
            gameNames = GetLinesFromFile(GAME_NAMES_FILE);
            realNames = GetLinesFromFile(REAL_NAMES_FILE);
            years = GetLinesFromFile(YEARS_FILE);

            // sort names to be in the same order as CarManager.AllCarsList
            List<string> newGameNames = new List<string>();
            CarManager.AllCarsList.ForEach(car => newGameNames.Add(car.name));

            List<string> newRealNames = new List<string>();
            List<string> newYears = new List<string>();
            newGameNames.ForEach(name =>
            {
                int newIndex = gameNames.IndexOf(name);
                newRealNames.Add(realNames[newIndex]);
                newYears.Add(years[newIndex]);
            });

            gameNames = newGameNames;
            realNames = newRealNames;
            years = newYears;

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

        // only used by "ReplaceName" (description)
        internal static string SwitchName(string carName, Format format)
        {
            int carIndex = DetectCarName(carName);
            return BuildTotalName(carIndex, format);
        }

        public static string SwitchName(Car car, Format format)
        {
            int carIndex = DetectCarName(car);
            return BuildTotalName(carIndex, format);
        }

        static string BuildTotalName(int carIndex, Format format)
        {
            string result = string.Empty;

            // I wish I could use the switch expression...
            switch (format)
            {
                case Format.original:
                    result = gameNames[carIndex];
                    break;

                case Format.real:
                    result = realNames[carIndex];
                    break;

                case Format.original_year:
                    result = BuildName(gameNames[carIndex], years[carIndex]);
                    break;

                case Format.real_year:
                    result = BuildName(realNames[carIndex], years[carIndex]);
                    break;

                case Format.original_year_real:
                    result = BuildName(gameNames[carIndex], years[carIndex] + " " + realNames[carIndex]);
                    break;

                case Format.real_original_year:
                    result = BuildName(realNames[carIndex], "\"" + gameNames[carIndex] + "\" " + years[carIndex]);
                    break;
            }

            return result;

            string BuildName(string start, string second)
            {
                string name = start;

                if (Main.settings.lowerSize)
                    name += "<size=" + Main.settings.lowTextSize + ">";

                name += " ";

                if (Main.settings.parenthesis)
                    name += "(";

                name += second;

                if (Main.settings.parenthesis)
                    name += ")";

                if (Main.settings.lowerSize)
                    name += "</size>";

                return name;
            }
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

            Format targetFormat = Main.settings.nameFormat;

            if (targetFormat == Format.original_year_real)
                targetFormat = Format.original_year;

            if (targetFormat == Format.real_original_year)
                targetFormat = Format.real_year;

            if (!Main.enabled)
                targetFormat = Format.original;

            detectedNames.ForEach(name => description = description.Replace(name, SwitchName(name, targetFormat)));
            return description;
        }

        // only used by "RaplaceName" (this is only used with exact game names)
        internal static int DetectCarName(string carName) => gameNames.IndexOf(carName);

        // used by "Matching dates" mod
        public static int DetectCarName(Car car) => CarManager.AllCarsList.IndexOf(car);

        // used by "Matching dates" mod
        public static int GetCarYear(Car car) => int.Parse(years[DetectCarName(car)]);
    }
}
