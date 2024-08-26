using UnityModManagerNet;

namespace RealCarNames
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw(DrawType.Toggle)]
        public bool realNames;

        [Draw(DrawType.Toggle)]
        public bool withDates;

        public void OnChange() => Main.RefreshCarNames();
    }
}
