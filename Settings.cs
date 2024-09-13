using RealCarNames.Patches;
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

        [Draw(DrawType.Slider, Min = 15, Max = 30)]
        public int lowTextSize = 20;

        [Draw(DrawType.Slider, Min = 0, Max = 100)]
        public int extraLeaderboardSpacing;

        [Header("Debug")]
        public bool disableInfoLog;

        public void OnChange()
        {
            Main.RefreshCarNames();
            LeaderboardScreenUpdater_UpdateLeaderboardUI_Patch.RefreshLeaderboard();
            StageResults_UpdateStageResults_Patch.RefreshStageResults();
            SeasonStandingsScreen_Init_Patch.RefreshLeaderboard();
        }

        public override void Save(UnityModManager.ModEntry modEntry) => Save(this, modEntry);
    }
}
