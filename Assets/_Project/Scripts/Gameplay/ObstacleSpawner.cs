using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// The obstacle "field" (MVP). Pools obstacles, advances them toward the hero,
    /// and resolves swipes against the frontmost obstacle in the strike zone. It no
    /// longer decides *what* or *when* to spawn — the SpawnDirector drives that and
    /// calls <see cref="SpawnThreat"/>. Builds placeholder cube obstacles at runtime.
    /// </summary>
    public class ObstacleSpawner : MonoBehaviour
    {
        public static ObstacleSpawner Instance { get; private set; }

        readonly List<Obstacle> _active = new List<Obstacle>();
        readonly Queue<Obstacle> _pool = new Queue<Obstacle>();

        float _burnTimer;      // Blaze ult: auto-incinerate in-zone threats
        Color _burnColor;
        float _freezeTimer;    // Frost ult: threats hold position

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart += ClearField;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart -= ClearField;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            // Blaze ult: incinerate anything in the strike zone.
            if (_burnTimer > 0f)
            {
                _burnTimer -= Time.deltaTime;
                BurnTick();
            }

            // Frost ult: threats hold position while frozen.
            bool frozen = _freezeTimer > 0f;
            if (frozen) _freezeTimer -= Time.deltaTime;

            // Advance active obstacles; handle ones that reached the hero.
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                if (frozen) continue; // frozen: no movement, no stagger tick, no damage
                if (o.Advance(gm.Speed))
                {
                    _active.RemoveAt(i);
                    Recycle(o);
                    gm.Damage();
                    CameraShake.Shake(0.35f);
                }
            }
        }

        /// <summary>Blaze ult — destroy every in-zone threat this frame as a scored kill.</summary>
        void BurnTick()
        {
            float maxZ = StrikeMaxZ();
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                if (!o.InZone(maxZ)) continue;
                Vector3 pos = o.transform.position;
                _active.RemoveAt(i);
                Recycle(o);
                // No drops from ult kills — the ult shouldn't recharge itself.
                int gain = GameManager.Instance.AddScore(Config.ScorePerKill * ComboManager.Instance.Multiplier);
                Vfx.Pop(pos, _burnColor);
                ScorePopup.Show(pos, "+" + gain, _burnColor, 0.8f);
            }
        }

        public void BurnFor(float seconds, Color color)
        {
            _burnTimer = seconds;
            _burnColor = color;
        }

        public void FreezeFor(float seconds) => _freezeTimer = seconds;

        public void SpawnThreat(SwipeDirection input, int lane) =>
            SpawnThreat(input, lane, SwipeDirection.None);

        /// <summary>
        /// Spawn one threat. Lane/height are derived from the FIRST input per the
        /// taxonomy (GDD §6–7): Up=high, Down=low, Left/Right/Tap force their lane at
        /// mid height. <paramref name="lane"/> only applies to Up/Down. A non-None
        /// <paramref name="second"/> makes it a two-step obstacle (GDD §7.1). Called
        /// by the SpawnDirector.
        /// </summary>
        public void SpawnThreat(SwipeDirection input, int lane, SwipeDirection second)
        {
            int useLane;
            HeightLevel height;

            switch (input)
            {
                case SwipeDirection.Up:    useLane = Mathf.Clamp(lane, 0, 2); height = HeightLevel.High; break;
                case SwipeDirection.Down:  useLane = Mathf.Clamp(lane, 0, 2); height = HeightLevel.Low;  break;
                case SwipeDirection.Left:  useLane = Config.LaneLeft;          height = HeightLevel.Mid;  break;
                case SwipeDirection.Right: useLane = Config.LaneRight;         height = HeightLevel.Mid;  break;
                default:                   useLane = Config.LaneCenter;        height = HeightLevel.Mid;  break; // Tap
            }

            Obstacle o = Get();
            o.Spawn(useLane, height, input, second);
            _active.Add(o);
        }

        // Far edge of the strike band, scaled by the active hero's input window
        // (GDD §8: Kremen forgives slow reads with a wider window).
        float StrikeMaxZ()
        {
            float mult = HeroController.Instance != null ? HeroController.Instance.InputWindowMult : 1f;
            return Config.StrikeZoneMax * mult;
        }

        /// <summary>Resolve a swipe against the frontmost obstacle in the strike zone.</summary>
        public void Resolve(SwipeDirection dir)
        {
            float maxZ = StrikeMaxZ();
            Obstacle target = null;
            float bestZ = float.MaxValue;
            foreach (Obstacle o in _active)
            {
                if (!o.InZone(maxZ)) continue;
                if (o.Z < bestZ) { bestZ = o.Z; target = o; }
            }

            if (target != null && target.CurrentInput == dir)
            {
                bool twoStep = target.IsTwoStep;

                // Two-step: the first correct swipe only opens the stagger window.
                if (!target.AdvanceStage())
                {
                    var h = HeroController.Instance;
                    Vector3 stagePos = target.transform.position;
                    Vfx.Pop(stagePos, h != null ? h.ElementTint : MaterialUtil.ForDirection(dir));
                    CameraShake.Shake(0.06f);
                    return; // combo intact, no score yet — finish it during the stagger
                }

                // Resolved: single kill, or two-step completed.
                _active.Remove(target);
                Vector3 pos = target.transform.position;
                Recycle(target);

                int mult = ComboManager.Instance.Multiplier;
                int gain = GameManager.Instance.AddScore(Config.ScorePerKill * mult);
                ComboManager.Instance.RegisterKill();

                if (twoStep)
                {
                    // Mastery reward: bonus score + an extra streak tick (GDD §7.1).
                    gain += GameManager.Instance.AddScore(Config.TwoStepBonus * mult);
                    ComboManager.Instance.RegisterKill();
                }

                ScorePopup.Show(pos, "+" + gain,
                    twoStep ? new Color(1f, 0.9f, 0.3f) : Color.white,
                    twoStep ? 1.3f : 1f);
                OrbManager.Instance?.TryDropAt(pos, guaranteed: twoStep);

                // Hero applies the flavour: element VFX, hit-stop, ult charge, passive.
                var hero = HeroController.Instance;
                if (hero != null && hero.Data != null)
                {
                    float earliness = Mathf.InverseLerp(Config.StrikeZoneMin, maxZ, pos.z);
                    var ctx = new AbilityContext { hero = hero, input = dir, position = pos, earliness = earliness };
                    hero.OnStrikeSuccess(in ctx);
                }
                else
                {
                    CameraShake.Shake(0.12f);
                    Vfx.Pop(pos, MaterialUtil.ForDirection(dir));
                }
            }
            else
            {
                // Wrong direction (or nothing in zone): lose a little combo, no damage.
                ComboManager.Instance.Whiff();
            }
        }

        /// <summary>
        /// Passive arc (Volt): destroy the nearest in-zone threat to a point for a
        /// bonus kill. Returns true if something was chained.
        /// </summary>
        public bool TryChainKill(Vector3 near, Color color)
        {
            float maxZ = StrikeMaxZ();
            Obstacle best = null;
            float bestSqr = float.MaxValue;
            foreach (Obstacle o in _active)
            {
                if (!o.InZone(maxZ)) continue;
                float d = (o.transform.position - near).sqrMagnitude;
                if (d < bestSqr) { bestSqr = d; best = o; }
            }
            if (best == null) return false;

            _active.Remove(best);
            Vector3 pos = best.transform.position;
            Recycle(best);

            int gain = GameManager.Instance.AddScore(Config.ScorePerKill * ComboManager.Instance.Multiplier);
            ComboManager.Instance.RegisterKill();
            Vfx.Pop(pos, color);
            ScorePopup.Show(pos, "+" + gain, color, 0.85f);
            OrbManager.Instance?.TryDropAt(pos);
            CameraShake.Shake(0.1f);
            return true;
        }

        /// <summary>Ultimate screen-clear: destroy every active threat as a scored kill.</summary>
        public int ClearAllAsKills(Color color)
        {
            int n = _active.Count;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                Vector3 pos = o.transform.position;
                Recycle(o);
                // No drops from ult kills — the ult shouldn't recharge itself.
                int gain = GameManager.Instance.AddScore(Config.ScorePerKill * ComboManager.Instance.Multiplier);
                Vfx.Pop(pos, color);
                ScorePopup.Show(pos, "+" + gain, color, 0.8f);
            }
            _active.Clear();
            return n;
        }

        void ClearField()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Recycle(_active[i]);
            _active.Clear();
            _burnTimer = 0f;
            _freezeTimer = 0f;
        }

        // ---- pooling ----
        Obstacle Get() => _pool.Count > 0 ? _pool.Dequeue() : Create();

        void Recycle(Obstacle o)
        {
            o.Deactivate();
            _pool.Enqueue(o);
        }

        Obstacle Create()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Obstacle";
            go.transform.localScale = Vector3.one * 0.95f;
            go.transform.SetParent(transform);
            Destroy(go.GetComponent<Collider>()); // not needed for this resolution model
            var o = go.AddComponent<Obstacle>();
            go.SetActive(false);
            return o;
        }
    }
}
