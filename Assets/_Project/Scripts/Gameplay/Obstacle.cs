using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// A threat moving toward the hero. Single-stage obstacles need one swipe (input
    /// from position/colour, GDD §6). Two-stage obstacles (GDD §7.1) need two swipes
    /// in order: landing the first opens a short "stagger" window — telegraphed by a
    /// recolour + pulse — during which the second swipe finishes it. Miss the window
    /// and progress reverts (no hit, per GDD). Pooled by ObstacleSpawner.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public bool Active { get; private set; }

        /// <summary>The input needed right now (stage 0 = first, stagger = second).</summary>
        public SwipeDirection CurrentInput { get; private set; }

        public bool IsTwoStep => _stageCount > 1;
        public bool Staggered => _staggerTimer > 0f;

        SwipeDirection _first, _second;
        int _stageCount;   // 1 or 2
        int _stage;        // stages completed so far
        float _staggerTimer;

        Renderer _renderer;
        Transform _t;
        Vector3 _baseScale;

        void Awake()
        {
            _t = transform;
            _renderer = GetComponentInChildren<Renderer>();
        }

        public void Spawn(int lane, HeightLevel height, SwipeDirection first, SwipeDirection second)
        {
            _first = first;
            _second = second;
            _stageCount = second == SwipeDirection.None ? 1 : 2;
            _stage = 0;
            _staggerTimer = 0f;
            CurrentInput = _first;

            float x = Config.LaneX[lane];
            float y = height == HeightLevel.Low ? Config.HeightLow
                    : height == HeightLevel.High ? Config.HeightHigh
                    : Config.HeightMid;
            _t.position = new Vector3(x, y, Config.SpawnZ);

            // Two-step reads as a bigger, special silhouette before you engage it.
            _baseScale = Vector3.one * (IsTwoStep ? Config.TwoStepScale : 0.95f);
            _t.localScale = _baseScale;

            Recolor();
            gameObject.SetActive(true);
            Active = true;
        }

        /// <summary>Apply the correct input for the current stage. Returns true when fully resolved.</summary>
        public bool AdvanceStage()
        {
            _stage++;
            if (_stage >= _stageCount) return true;   // single hit, or two-step finished

            // First stage landed → vulnerable stagger window for the second input.
            CurrentInput = _second;
            _staggerTimer = Config.StaggerWindow;
            Recolor();
            return false;
        }

        /// <summary>Move toward the hero, tick the stagger. Returns true if it reached the hero.</summary>
        public bool Advance(float speed)
        {
            _t.position += Vector3.back * speed * Time.deltaTime;

            if (_staggerTimer > 0f)
            {
                _staggerTimer -= Time.deltaTime;
                // Pulse to scream "react now".
                float pulse = 1f + Mathf.Sin(Time.time * 30f) * 0.12f;
                _t.localScale = _baseScale * pulse;
                if (_staggerTimer <= 0f) RevertStage();
            }

            return _t.position.z <= Config.HeroZ;
        }

        // Failed the second input in time: progress fades back to stage one (not a hit).
        void RevertStage()
        {
            _stage = 0;
            _staggerTimer = 0f;
            CurrentInput = _first;
            _t.localScale = _baseScale;
            Recolor();
        }

        void Recolor()
        {
            if (_renderer != null) _renderer.material.color = MaterialUtil.ForDirection(CurrentInput);
        }

        public float Z => _t.position.z;

        /// <summary>In the strike band, whose far edge scales with the hero's input window.</summary>
        public bool InZone(float maxZ) => _t.position.z >= Config.StrikeZoneMin && _t.position.z <= maxZ;

        public void Deactivate()
        {
            Active = false;
            _staggerTimer = 0f;
            gameObject.SetActive(false);
        }
    }
}
