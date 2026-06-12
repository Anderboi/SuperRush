using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// The hero stays centred (core fantasy: attack, don't dodge). It hosts the hero's
    /// data-driven abilities (GDD §8, §14.2): routes the five inputs to the spawner's
    /// resolver, applies the hero's strike flavour + passive on a kill, tracks the
    /// ultimate meter, and fires the ultimate. Also plays placeholder squash/flash feel.
    /// </summary>
    public class HeroController : MonoBehaviour
    {
        public static HeroController Instance { get; private set; }

        [Tooltip("Active hero. Defaults to the first of the roster; swap on the menu with Tab.")]
        public HeroData Data;

        List<HeroData> _roster;
        int _heroIndex;

        public int StrikeCount { get; private set; }
        public float Charge { get; private set; }

        public int MaxLives => Data != null ? Data.maxLives : Config.StartLives;
        public float InputWindowMult => Data != null ? Data.inputWindowMult : 1f;
        float ChargeMax => (Data != null && Data.ultimate != null) ? Data.ultimate.chargeMax : 100f;
        public float Charge01 => Mathf.Clamp01(Charge / ChargeMax);
        public bool UltReady => Charge >= ChargeMax;

        /// <summary>Hero element colour — used for stage-confirm flavour on two-step obstacles.</summary>
        public Color ElementTint => (Data != null && Data.strike != null) ? Data.strike.elementTint : Color.white;

        Renderer _renderer;
        Color _baseColor;
        Vector3 _baseScale;
        float _punch;   // attack squash timer
        float _flash;   // hurt flash timer

        void Awake()
        {
            Instance = this;
            if (_roster == null || _roster.Count == 0) _roster = HeroLibrary.Roster();
            if (Data == null) Data = _roster[0];
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null) _baseColor = _renderer.material.color;
            _baseScale = transform.localScale;
        }

        /// <summary>Cycle the active hero on the menu (GDD §8 roster; real picker is M3).</summary>
        public void CycleHero()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Menu) return;
            if (_roster == null || _roster.Count == 0) return;
            _heroIndex = (_heroIndex + 1) % _roster.Count;
            Data = _roster[_heroIndex];
        }

        void OnEnable()  => Subscribe();
        void Start()     => Subscribe();   // re-subscribe safely regardless of Awake ordering
        void OnDisable() => Unsubscribe();

        void Subscribe()
        {
            if (SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipe -= HandleSwipe;
                SwipeDetector.Instance.OnSwipe += HandleSwipe;
                SwipeDetector.Instance.OnUltimate -= TryActivateUltimate;
                SwipeDetector.Instance.OnUltimate += TryActivateUltimate;
                SwipeDetector.Instance.OnCycleHero -= CycleHero;
                SwipeDetector.Instance.OnCycleHero += CycleHero;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHit -= OnHit;
                GameManager.Instance.OnHit += OnHit;
                GameManager.Instance.OnRunStart -= ResetRun;
                GameManager.Instance.OnRunStart += ResetRun;
            }
        }

        void Unsubscribe()
        {
            if (SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipe -= HandleSwipe;
                SwipeDetector.Instance.OnUltimate -= TryActivateUltimate;
                SwipeDetector.Instance.OnCycleHero -= CycleHero;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHit -= OnHit;
                GameManager.Instance.OnRunStart -= ResetRun;
            }
        }

        void HandleSwipe(SwipeDirection dir)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            ObstacleSpawner.Instance.Resolve(dir);
            _punch = 0.12f;
        }

        /// <summary>Called by the spawner after a directional strike kills a threat.</summary>
        public void OnStrikeSuccess(in AbilityContext ctx)
        {
            StrikeCount++;
            if (Data != null && Data.strike != null) Data.strike.Execute(in ctx);
            if (Data != null && Data.passive != null) Data.passive.OnKill(in ctx);
        }

        public void AddCharge(float amount) => Charge = Mathf.Min(ChargeMax, Charge + amount);

        public void TryActivateUltimate()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            if (!UltReady || Data == null || Data.ultimate == null) return;

            var ctx = new AbilityContext { hero = this, input = SwipeDirection.None, position = transform.position };
            Data.ultimate.Activate(in ctx);
            Charge = 0f;
        }

        void ResetRun()
        {
            StrikeCount = 0;
            Charge = 0f;
        }

        void OnHit() => _flash = 0.18f;

        void Update()
        {
            // Squash on attack.
            float squash = _punch > 0f ? 1f + Mathf.Sin((0.12f - _punch) / 0.12f * Mathf.PI) * 0.18f : 1f;
            transform.localScale = new Vector3(_baseScale.x / squash, _baseScale.y * squash, _baseScale.z / squash);
            if (_punch > 0f) _punch -= Time.deltaTime;

            // Hurt flash.
            if (_renderer != null)
            {
                if (_flash > 0f)
                {
                    _flash -= Time.deltaTime;
                    _renderer.material.color = Color.Lerp(_baseColor, Color.red, Mathf.PingPong(Time.time * 12f, 1f));
                }
                else _renderer.material.color = _baseColor;
            }
        }
    }
}
