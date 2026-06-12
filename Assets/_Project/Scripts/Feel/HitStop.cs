using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Brief time freeze for hit crunch (GDD Feel: hit-stop via Time.timeScale). Call
    /// HitStop.Do(seconds) from anywhere; restores on its own using unscaled time, so
    /// it works even while timeScale is 0.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        static HitStop _instance;
        float _restoreAt = -1f;

        void Awake() => _instance = this;

        public static void Do(float seconds)
        {
            if (_instance == null)
            {
                var go = new GameObject("_HitStop");
                _instance = go.AddComponent<HitStop>();
            }
            _instance.Begin(seconds);
        }

        void Begin(float seconds)
        {
            Time.timeScale = 0f;
            _restoreAt = Time.realtimeSinceStartup + seconds;
        }

        void Update()
        {
            if (_restoreAt > 0f && Time.realtimeSinceStartup >= _restoreAt)
            {
                Time.timeScale = 1f;
                _restoreAt = -1f;
            }
        }
    }
}
