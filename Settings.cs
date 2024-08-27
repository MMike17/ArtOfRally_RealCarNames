using UnityEngine;
using UnityModManagerNet;

namespace RealCarNames
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        public enum Format
        {
            original,
            real,
            original_year,
            real_year,
            original_year_real,
            real_original_year
        };

        [Header("Only the four first options are valid in car descriptions.")]
        [Draw(DrawType.PopupList)]
        public Format nameFormat;

        [Draw(DrawType.Toggle)]
        public bool lowerSize;

        [Draw(DrawType.Toggle)]
        public bool parenthesis;

        [Draw(DrawType.Slider, Min = 5, Max = 20)] // <= decide that later
        public int lowTextSize = 10;

        public void OnChange() => Main.RefreshCarNames();

        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
    }
}
