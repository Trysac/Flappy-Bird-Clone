using FlappyDragon.Core;
using UnityEngine;

namespace FlappyDragon.Game
{
    /// <summary>Keeps the best score in PlayerPrefs (the registry on Windows, IndexedDB on the web).</summary>
    public sealed class PlayerPrefsBestScoreStore : IBestScoreStore
    {
        public const string Key = "FlappyDragon.BestScore";

        public int LoadBest() => PlayerPrefs.GetInt(Key, 0);

        public void SaveBest(int score)
        {
            PlayerPrefs.SetInt(Key, score);
            PlayerPrefs.Save();
        }
    }
}
