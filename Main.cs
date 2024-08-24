using UnityModManagerNet;

namespace RealCarNames
{
    public class Main
    {
        public static UnityModManager.ModEntry.ModLogger Logger;

        static int ewsfCount;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ewsfCount = 0;
            Logger = modEntry.Logger;

            CarNameProvider.Init();

            // test
            Log(CarNameProvider.GetCarName("the t22"));

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