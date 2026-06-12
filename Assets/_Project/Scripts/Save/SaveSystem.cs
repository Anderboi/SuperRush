using System;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Persistent profile (GDD §14.2). Data is a JSON blob stored via PlayerPrefs:
    /// JSON keeps the structure ready for a real file/cloud backend later, PlayerPrefs
    /// keeps it zero-setup and Web-build safe (GDD §14.4). Swap the backing store
    /// without touching callers.
    /// </summary>
    public static class SaveSystem
    {
        [Serializable]
        public class SaveData
        {
            public int bestScore;
            public float bestDistance;   // farthest push toward the Core (GDD §2)
            public int coins;            // banked across runs (economy proper is M3)
            public int runs;
        }

        const string Key = "ryvok_save";

        static SaveData _data;

        public static SaveData Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }

        static void Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            _data = string.IsNullOrEmpty(json) ? new SaveData()
                                               : JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        }

        public static void Save()
        {
            if (_data == null) return;
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(_data));
            PlayerPrefs.Save();
        }
    }
}
