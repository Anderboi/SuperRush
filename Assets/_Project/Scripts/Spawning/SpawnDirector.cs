using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Spawn Director (GDD §9, §14.2). Follows the difficulty ramp, picks authored
    /// patterns whose band matches the current run, and plays their slots over time
    /// by asking the ObstacleSpawner to spawn each threat. This replaces random
    /// one-by-one spawning with deliberate "designer waves".
    ///
    /// Assign SpawnPattern assets in the inspector to author waves, or leave the list
    /// empty and a default library is built in code so the zero-setup starter runs.
    /// </summary>
    public class SpawnDirector : MonoBehaviour
    {
        public static SpawnDirector Instance { get; private set; }

        [Tooltip("Authored patterns. Leave empty to use the built-in default library.")]
        public List<SpawnPattern> patterns = new List<SpawnPattern>();

        SpawnPattern _current;
        int _slot;
        float _timer;
        readonly List<SpawnPattern> _eligible = new List<SpawnPattern>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart += ResetRun;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart -= ResetRun;
        }

        /// <summary>Hold off spawning for a while (e.g. Volt's ultimate stun).</summary>
        public void Stun(float seconds)
        {
            _current = null;
            _slot = 0;
            _timer = Mathf.Max(_timer, seconds);
        }

        void Start()
        {
            if (patterns == null || patterns.Count == 0)
                patterns = BuildDefaultLibrary();
            ResetRun();
        }

        void ResetRun()
        {
            _current = null;
            _slot = 0;
            _timer = Config.StartGap; // brief lead-in before the first wave
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            // Pattern finished (or none yet): rest, then choose the next wave.
            if (_current == null || _slot >= _current.slots.Length)
            {
                _current = PickPattern(Difficulty(gm));
                _slot = 0;

                if (_current == null) { _timer = BeatSeconds(gm); return; } // no patterns: idle

                float lead = _current.slots[0].beats;
                _timer = (Config.PatternRestBeats + lead) * BeatSeconds(gm);
                return;
            }

            // Spawn the current slot, then schedule the next one.
            SpawnPattern.Slot s = _current.slots[_slot];
            ObstacleSpawner.Instance.SpawnThreat(s.input, s.lane);
            _slot++;

            if (_slot < _current.slots.Length)
            {
                float nextBeats = _current.slots[_slot].beats;
                _timer = nextBeats * BeatSeconds(gm) * Random.Range(0.95f, 1.1f);
            }
            else
            {
                _timer = 0f; // exhausted: next frame rests and picks a new pattern
            }
        }

        // 0 at start speed, 1 at max speed — the same ramp the rest of the game reads.
        static float Difficulty(GameManager gm) =>
            Mathf.InverseLerp(Config.StartSpeed, Config.MaxSpeed, gm.Speed);

        // One beat in seconds, tied to the difficulty gap so waves tighten with speed.
        static float BeatSeconds(GameManager gm) =>
            Mathf.Lerp(Config.StartGap, Config.MinGap, Difficulty(gm));

        SpawnPattern PickPattern(float diff)
        {
            _eligible.Clear();
            foreach (SpawnPattern p in patterns)
                if (p != null && p.IsPlayable && p.FitsDifficulty(diff))
                    _eligible.Add(p);

            // Fallback: nothing fits this difficulty exactly → allow any playable one.
            if (_eligible.Count == 0)
                foreach (SpawnPattern p in patterns)
                    if (p != null && p.IsPlayable)
                        _eligible.Add(p);

            if (_eligible.Count == 0) return null;
            return _eligible[Random.Range(0, _eligible.Count)];
        }

        // ---- default library (so the starter runs with no authored assets) ----
        static List<SpawnPattern> BuildDefaultLibrary()
        {
            var list = new List<SpawnPattern>
            {
                // Easy — one read at a time, generous spacing.
                Make("Intro-Tap",   0f, 0.35f, S(SwipeDirection.Tap, 1, 1f)),
                Make("Intro-Drone", 0f, 0.35f, S(SwipeDirection.Up, 1, 1f)),
                Make("Left-Right",  0f, 0.5f,
                    S(SwipeDirection.Left, 0, 1f),
                    S(SwipeDirection.Right, 2, 1f)),
                Make("Low-Roll",    0f, 0.5f,
                    S(SwipeDirection.Down, 0, 1f),
                    S(SwipeDirection.Down, 2, 1f)),

                // Medium — short sequences that scan height/lanes.
                Make("Vertical-Scan", 0.25f, 0.7f,
                    S(SwipeDirection.Down, 1, 1f),
                    S(SwipeDirection.Tap, 1, 0.9f),
                    S(SwipeDirection.Up, 1, 0.9f)),
                Make("Zigzag", 0.3f, 0.75f,
                    S(SwipeDirection.Left, 0, 0.9f),
                    S(SwipeDirection.Right, 2, 0.85f),
                    S(SwipeDirection.Left, 0, 0.85f),
                    S(SwipeDirection.Right, 2, 0.85f)),
                Make("Tap-Burst", 0.3f, 0.75f,
                    S(SwipeDirection.Tap, 1, 0.85f),
                    S(SwipeDirection.Tap, 1, 0.85f),
                    S(SwipeDirection.Up, 1, 0.85f)),

                // Hard — five-beat scrambles, tighter spacing.
                Make("Scramble", 0.55f, 1f,
                    S(SwipeDirection.Up, 0, 0.8f),
                    S(SwipeDirection.Left, 0, 0.75f),
                    S(SwipeDirection.Tap, 1, 0.75f),
                    S(SwipeDirection.Right, 2, 0.75f),
                    S(SwipeDirection.Down, 1, 0.75f)),
                Make("Rapid-Lanes", 0.6f, 1f,
                    S(SwipeDirection.Left, 0, 0.7f),
                    S(SwipeDirection.Right, 2, 0.7f),
                    S(SwipeDirection.Up, 1, 0.7f),
                    S(SwipeDirection.Down, 1, 0.7f),
                    S(SwipeDirection.Tap, 1, 0.7f)),
            };
            return list;
        }

        static SpawnPattern.Slot S(SwipeDirection input, int lane, float beats) =>
            new SpawnPattern.Slot(input, lane, beats);

        static SpawnPattern Make(string label, float min, float max, params SpawnPattern.Slot[] slots)
        {
            var p = ScriptableObject.CreateInstance<SpawnPattern>();
            p.label = label;
            p.minDifficulty = min;
            p.maxDifficulty = max;
            p.slots = slots;
            return p;
        }
    }
}
