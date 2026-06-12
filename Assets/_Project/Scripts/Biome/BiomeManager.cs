using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Drives biome transitions (GDD §11, §2). Watches the run distance, activates
    /// the next BiomeData when the hero crosses its triggerDistance, then smoothly
    /// lerps the camera background color, ground material, stripe materials, and
    /// RenderSettings fog toward the new palette.
    ///
    /// Leave biomes empty and the default BiomeLibrary is used (zero-setup).
    /// Assign materials via Bootstrap or let this component discover the Ground
    /// plane and GroundScroller stripes at runtime.
    /// </summary>
    public class BiomeManager : MonoBehaviour
    {
        public static BiomeManager Instance { get; private set; }

        [Tooltip("Authored biomes sorted by triggerDistance. Leave empty for defaults.")]
        public List<BiomeData> biomes = new List<BiomeData>();

        // Runtime state
        BiomeData _current;
        BiomeData _target;
        float     _lerpT;       // 0..1 progress of the active transition
        float     _lerpSpeed;   // 1 / transitionSeconds

        // Scene references populated in Init().
        Camera        _cam;
        Renderer      _groundRend;
        Material      _groundMat;
        List<Material> _stripeMats = new List<Material>();

        // ── palette snapshots for the lerp ────────────────────────────────────
        Color _fromSky, _fromGround, _fromStripe, _fromFog;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void Start()
        {
            if (biomes == null || biomes.Count == 0)
            {
                biomes = new List<BiomeData>(BiomeLibrary.Default());
            }
            biomes.Sort((a, b) => a.triggerDistance.CompareTo(b.triggerDistance));

            GatherSceneRefs();

            // Apply the first biome instantly (transitionSeconds == 0).
            if (biomes.Count > 0)
                ApplyInstant(biomes[0]);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart += OnRunStart;
                GameManager.Instance.OnRunEnd   += OnRunEnd;
            }
        }

        void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= OnRunStart;
                GameManager.Instance.OnRunEnd   -= OnRunEnd;
            }
        }

        void OnRunStart()
        {
            // Always snap back to biome 0 at the start of a new run.
            if (biomes.Count > 0) ApplyInstant(biomes[0]);
            _target = null;
            _lerpT = 1f;
        }

        void OnRunEnd() { /* nothing — keep the palette for the results screen */ }

        void GatherSceneRefs()
        {
            _cam = Camera.main;

            // Ground plane created by Bootstrap.
            var groundGo = GameObject.Find("Ground");
            if (groundGo != null)
            {
                _groundRend = groundGo.GetComponent<Renderer>();
                if (_groundRend != null) _groundMat = _groundRend.material;
            }

            // Stripe quads created by GroundScroller.
            var scroller = FindObjectOfType<GroundScroller>();
            if (scroller != null)
            {
                foreach (Transform child in scroller.transform)
                {
                    var r = child.GetComponent<Renderer>();
                    if (r != null) _stripeMats.Add(r.material);
                }
            }
        }

        // ── per-frame ─────────────────────────────────────────────────────────
        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            CheckTrigger(gm.Distance);
            TickLerp();
        }

        void CheckTrigger(float dist)
        {
            // Walk backwards through sorted biomes to find the latest one that has
            // fired. If it's not already the current/target, begin the transition.
            BiomeData due = null;
            for (int i = biomes.Count - 1; i >= 0; i--)
            {
                if (biomes[i].triggerDistance <= dist)
                {
                    due = biomes[i];
                    break;
                }
            }

            if (due == null || due == _current || due == _target) return;

            // Snapshot current colors and start lerping toward the new palette.
            _fromSky    = _cam != null ? _cam.backgroundColor : Color.black;
            _fromGround = _groundMat != null ? _groundMat.color : Color.white;
            _fromStripe = _stripeMats.Count > 0 ? _stripeMats[0].color : Color.white;
            _fromFog    = RenderSettings.fogColor;

            _target    = due;
            _lerpT     = 0f;
            _lerpSpeed = due.transitionSeconds > 0f ? 1f / due.transitionSeconds : 999f;
        }

        void TickLerp()
        {
            if (_target == null || _lerpT >= 1f) return;

            _lerpT = Mathf.MoveTowards(_lerpT, 1f, _lerpSpeed * Time.deltaTime);
            float t = Mathf.SmoothStep(0f, 1f, _lerpT);

            ApplyColors(
                Color.Lerp(_fromSky,    _target.skyColor,    t),
                Color.Lerp(_fromGround, _target.groundColor, t),
                Color.Lerp(_fromStripe, _target.stripeColor, t),
                Color.Lerp(_fromFog,    _target.fogColor,    t));

            if (_lerpT >= 1f) _current = _target;
        }

        // ── helpers ───────────────────────────────────────────────────────────
        void ApplyInstant(BiomeData b)
        {
            _current = b;
            ApplyColors(b.skyColor, b.groundColor, b.stripeColor, b.fogColor);
        }

        void ApplyColors(Color sky, Color ground, Color stripe, Color fog)
        {
            if (_cam != null) _cam.backgroundColor = sky;

            if (_groundMat != null)
            {
                _groundMat.color = ground;
                if (_groundMat.HasProperty("_BaseColor")) _groundMat.SetColor("_BaseColor", ground);
            }

            foreach (var m in _stripeMats)
            {
                m.color = stripe;
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", stripe);
            }

            RenderSettings.fogColor = fog;
        }

        // ── public read (HUD, etc.) ────────────────────────────────────────────
        public string CurrentBiomeName => _current != null ? _current.biomeName : "";
        public bool   InTransition     => _target != null && _lerpT < 1f;
    }
}
