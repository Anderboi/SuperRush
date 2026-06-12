using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// A single threat moving toward the hero. Its RequiredInput is determined by
    /// position (height/lane) per GDD section 6, and telegraphed by colour.
    /// Pooled by ObstacleSpawner — never Destroy()ed at runtime.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        public SwipeDirection RequiredInput { get; private set; }
        public bool Active { get; private set; }

        Renderer _renderer;
        Transform _t;

        void Awake()
        {
            _t = transform;
            _renderer = GetComponentInChildren<Renderer>();
        }

        public void Spawn(int lane, HeightLevel height, SwipeDirection required)
        {
            RequiredInput = required;
            float x = Config.LaneX[lane];
            float y = height == HeightLevel.Low ? Config.HeightLow
                    : height == HeightLevel.High ? Config.HeightHigh
                    : Config.HeightMid;

            _t.position = new Vector3(x, y, Config.SpawnZ);
            if (_renderer != null) _renderer.material.color = MaterialUtil.ForDirection(required);
            gameObject.SetActive(true);
            Active = true;
        }

        /// <summary>Move toward the hero. Returns true if it reached the hero (a hit).</summary>
        public bool Advance(float speed)
        {
            _t.position += Vector3.back * speed * Time.deltaTime;
            return _t.position.z <= Config.HeroZ;
        }

        public float Z => _t.position.z;

        public bool InStrikeZone =>
            _t.position.z >= Config.StrikeZoneMin && _t.position.z <= Config.StrikeZoneMax;

        public void Deactivate()
        {
            Active = false;
            gameObject.SetActive(false);
        }
    }
}
