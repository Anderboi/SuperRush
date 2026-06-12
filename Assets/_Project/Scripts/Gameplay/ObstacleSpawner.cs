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

            // Advance active obstacles; handle ones that reached the hero.
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Obstacle o = _active[i];
                if (o.Advance(gm.Speed))
                {
                    _active.RemoveAt(i);
                    Recycle(o);
                    gm.Damage();
                    CameraShake.Shake(0.35f);
                }
            }
        }

        /// <summary>
        /// Spawn one threat for the given input. Lane/height are derived from the
        /// input per the taxonomy (GDD §6–7): Up=high, Down=low, Left/Right/Tap force
        /// their lane at mid height. <paramref name="lane"/> only applies to Up/Down.
        /// Called by the SpawnDirector.
        /// </summary>
        public void SpawnThreat(SwipeDirection input, int lane)
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
            o.Spawn(useLane, height, input);
            _active.Add(o);
        }

        /// <summary>Resolve a swipe against the frontmost obstacle in the strike zone.</summary>
        public void Resolve(SwipeDirection dir)
        {
            Obstacle target = null;
            float bestZ = float.MaxValue;
            foreach (Obstacle o in _active)
            {
                if (!o.InStrikeZone) continue;
                if (o.Z < bestZ) { bestZ = o.Z; target = o; }
            }

            if (target != null && target.RequiredInput == dir)
            {
                _active.Remove(target);
                Vector3 pos = target.transform.position;
                Recycle(target);

                int gain = Config.ScorePerKill * ComboManager.Instance.Multiplier;
                GameManager.Instance.AddScore(gain);
                ComboManager.Instance.RegisterKill();
                CameraShake.Shake(0.12f);
                Vfx.Pop(pos, MaterialUtil.ForDirection(dir));
            }
            else
            {
                // Wrong direction (or nothing in zone): lose a little combo, no damage.
                ComboManager.Instance.Whiff();
            }
        }

        void ClearField()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Recycle(_active[i]);
            _active.Clear();
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
