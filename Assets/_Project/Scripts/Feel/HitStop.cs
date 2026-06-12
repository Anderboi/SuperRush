using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Single owner of Time.timeScale for game feel (GDD Feel): hit-stop freeze and
    /// Frost's slow-mo. Call HitStop.Do(seconds) for a freeze or HitStop.SlowMo(scale,
    /// seconds) for slow motion. Always restores to 1 using unscaled time, so it works
    /// even while timeScale is 0. Latest call wins.
    /// </summary>
    public class HitStop : MonoBehaviour
    {
        static HitStop _instance;
        float _restoreAt = -1f;

        void Awake() => _instance = this;

        /// <summary>Hard freeze for hit crunch.</summary>
        public static void Do(float seconds) => Set(0f, seconds);

        /// <summary>Slow motion at <paramref name="scale"/> (0..1) for a real-time window.</summary>
        public static void SlowMo(float scale, float seconds) => Set(Mathf.Clamp01(scale), seconds);

        static void Set(float scale, float seconds)
        {
            if (_instance == null)
            {
                var go = new GameObject("_HitStop");
                _instance = go.AddComponent<HitStop>();
            }
            _instance.Begin(scale, seconds);
        }

        void Begin(float scale, float seconds)
        {
            Time.timeScale = scale;
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
