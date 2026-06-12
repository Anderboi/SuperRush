using UnityEngine;

namespace Ryvok
{
    public enum OrbType { Energy, Coin }

    /// <summary>
    /// A pickup shed by a destroyed threat (GDD §7): scatters for a beat, then homes
    /// to the hero. Energy orbs pay score + ult charge; coins pay run currency.
    /// Pooled by OrbManager — never Destroy()ed at runtime.
    /// </summary>
    public class Orb : MonoBehaviour
    {
        public bool Active { get; private set; }

        OrbType _type;
        Vector3 _velocity;
        float _age;
        Transform _t;
        Renderer _renderer;

        const float ScatterTime = 0.28f;   // free flight before homing kicks in
        const float HomeAccel = 55f;       // homing acceleration
        const float CollectDist = 0.6f;
        const float MaxLife = 4f;          // safety: collect no matter what

        void Awake()
        {
            _t = transform;
            _renderer = GetComponentInChildren<Renderer>();
        }

        public void Spawn(Vector3 pos, OrbType type)
        {
            _type = type;
            _age = 0f;
            _t.position = pos;
            _t.localScale = Vector3.one * (type == OrbType.Coin ? 0.32f : 0.22f);
            // Pop outward in a random direction, slightly upward.
            _velocity = new Vector3(Random.Range(-3f, 3f), Random.Range(2f, 5f), Random.Range(-2f, 1f));
            if (_renderer != null)
                _renderer.material.color = type == OrbType.Coin ? MaterialUtil.CoinColor : MaterialUtil.OrbColor;
            gameObject.SetActive(true);
            Active = true;
        }

        /// <summary>Advance the orb. Returns true when collected (or expired).</summary>
        public bool Tick()
        {
            _age += Time.deltaTime;
            var hero = HeroController.Instance;
            if (hero == null || _age >= MaxLife) { Collect(); return true; }

            Vector3 target = hero.transform.position + Vector3.up * 0.8f;

            if (_age < ScatterTime)
            {
                _velocity += Physics.gravity * (0.5f * Time.deltaTime);
            }
            else
            {
                // Accelerate toward the hero, capping turn speed for a nice swoop.
                Vector3 dir = (target - _t.position).normalized;
                _velocity += dir * (HomeAccel * Time.deltaTime);
                _velocity = Vector3.ClampMagnitude(_velocity, 26f);
            }

            _t.position += _velocity * Time.deltaTime;

            if ((target - _t.position).sqrMagnitude < CollectDist * CollectDist)
            {
                Collect();
                return true;
            }
            return false;
        }

        void Collect()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            if (_type == OrbType.Energy)
            {
                gm.AddScore(Config.OrbScore);
                HeroController.Instance?.AddCharge(Config.OrbCharge);
            }
            else
            {
                gm.AddCoins(1);
            }
        }

        public void Deactivate()
        {
            Active = false;
            gameObject.SetActive(false);
        }
    }
}
