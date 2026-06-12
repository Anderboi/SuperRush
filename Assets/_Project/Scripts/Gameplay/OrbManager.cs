using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Pools and drives pickup orbs (GDD §7 drops). Kill sites call
    /// <see cref="TryDropAt"/>; the roll and the coin bonus live here so tuning stays
    /// in one place. Builds placeholder sphere orbs at runtime.
    /// </summary>
    public class OrbManager : MonoBehaviour
    {
        public static OrbManager Instance { get; private set; }

        readonly List<Orb> _active = new List<Orb>();
        readonly Queue<Orb> _pool = new Queue<Orb>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart += ClearAll;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnRunStart -= ClearAll;
        }

        void Update()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Tick())
                {
                    Orb o = _active[i];
                    _active.RemoveAt(i);
                    Recycle(o);
                }
            }
        }

        /// <summary>
        /// Roll a drop at a kill position. <paramref name="guaranteed"/> skips the
        /// chance roll (two-step mastery reward).
        /// </summary>
        public void TryDropAt(Vector3 pos, bool guaranteed = false)
        {
            if (!guaranteed && Random.value > Config.OrbDropChance) return;

            for (int i = 0; i < Config.OrbsPerDrop; i++)
                Spawn(pos, OrbType.Energy);

            if (Random.value < Config.CoinDropChance)
                Spawn(pos, OrbType.Coin);
        }

        void Spawn(Vector3 pos, OrbType type)
        {
            Orb o = _pool.Count > 0 ? _pool.Dequeue() : Create();
            o.Spawn(pos, type);
            _active.Add(o);
        }

        void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Recycle(_active[i]);
            _active.Clear();
        }

        void Recycle(Orb o)
        {
            o.Deactivate();
            _pool.Enqueue(o);
        }

        Orb Create()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Orb";
            go.transform.SetParent(transform);
            Destroy(go.GetComponent<Collider>()); // collection is distance-based
            var o = go.AddComponent<Orb>();
            go.SetActive(false);
            return o;
        }
    }
}
