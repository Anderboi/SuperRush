using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Drives boss encounters (GDD §7, M2 milestone). A boss spawns every
    /// Config.BossInterval metres: it parks at Config.BossZ, fires scripted BossPattern
    /// volleys, then opens a weak-spot window. The hero hits the weak spot
    /// pattern.hitsToBreak times to break a phase; all phases down → boss dies.
    ///
    /// While the boss is alive the SpawnDirector is stunned (no normal waves) and the
    /// speed ramp pauses, giving the player a clean arena fight. A boss kill rewards a
    /// large score bonus and drops a guaranteed orb burst.
    /// </summary>
    public class BossSystem : MonoBehaviour
    {
        public static BossSystem Instance { get; private set; }

        [Tooltip("Boss phase definitions. Leave empty to use the built-in default.")]
        public BossPattern[] phases;

        public bool BossActive { get; private set; }

        // ── state machine ──────────────────────────────────────────────────────
        enum Phase { Idle, Approach, Volley, Vulnerable, Dead }
        Phase _phase = Phase.Idle;

        int _phaseIndex;       // which BossPattern we're on
        int _hitsThisPhase;    // hits landed on current weak spot
        float _timer;
        int _volleySlot;       // next volley shot to fire

        // ── boss visual (primitive stand-in) ──────────────────────────────────
        GameObject _bossGo;
        Renderer _bossRend;
        GameObject _weakSpotGo;
        Renderer _weakSpotRend;

        // ── next trigger ──────────────────────────────────────────────────────
        float _nextTriggerDist;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            if (phases == null || phases.Length == 0) phases = BossLibrary.DefaultPhases();
        }

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart += OnRunStart;
                GameManager.Instance.OnRunEnd   += OnRunEnd;
            }
            if (SwipeDetector.Instance != null)
                SwipeDetector.Instance.OnSwipe += OnSwipe;
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= OnRunStart;
                GameManager.Instance.OnRunEnd   -= OnRunEnd;
            }
            if (SwipeDetector.Instance != null)
                SwipeDetector.Instance.OnSwipe -= OnSwipe;
        }

        void Start()
        {
            // Late re-subscribe so ordering doesn't matter.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= OnRunStart;
                GameManager.Instance.OnRunStart += OnRunStart;
                GameManager.Instance.OnRunEnd   -= OnRunEnd;
                GameManager.Instance.OnRunEnd   += OnRunEnd;
            }
            if (SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipe -= OnSwipe;
                SwipeDetector.Instance.OnSwipe += OnSwipe;
            }
        }

        // ── lifecycle ──────────────────────────────────────────────────────────
        void OnRunStart()
        {
            _nextTriggerDist = Config.BossInterval;
            _phase = Phase.Idle;
            BossActive = false;
            HideBoss();
        }

        void OnRunEnd() { HideBoss(); _phase = Phase.Idle; BossActive = false; }

        // ── update ─────────────────────────────────────────────────────────────
        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;

            // Trigger next encounter.
            if (_phase == Phase.Idle && gm.Distance >= _nextTriggerDist)
                StartEncounter();

            switch (_phase)
            {
                case Phase.Approach:    TickApproach();    break;
                case Phase.Volley:      TickVolley();      break;
                case Phase.Vulnerable:  TickVulnerable();  break;
            }
        }

        // ── encounter start ────────────────────────────────────────────────────
        void StartEncounter()
        {
            BossActive = true;
            _phaseIndex = 0;
            _hitsThisPhase = 0;
            _phase = Phase.Approach;
            _timer = 0f;

            SpawnDirector.Instance?.Stun(999f);   // hold normal waves for the whole fight
            ObstacleSpawner.Instance?.ClearAllAsKills(Color.white);  // sweep lingering threats

            BuildBossVisual();
            _bossGo.transform.position = new Vector3(0f, 1.4f, Config.BossZ + 20f); // starts farther
        }

        // ── Approach: boss slides in to its fighting position ──────────────────
        void TickApproach()
        {
            float targetZ = Config.BossZ;
            Vector3 pos = _bossGo.transform.position;
            float newZ = Mathf.MoveTowards(pos.z, targetZ, 12f * Time.deltaTime);
            _bossGo.transform.position = new Vector3(pos.x, pos.y, newZ);
            if (Mathf.Abs(newZ - targetZ) < 0.1f) BeginVolley();
        }

        // ── Volley: fire the scripted barrage ──────────────────────────────────
        void BeginVolley()
        {
            _phase = Phase.Volley;
            _volleySlot = 0;
            _timer = 0f;
        }

        void TickVolley()
        {
            if (_phaseIndex >= phases.Length) return;
            var pat = phases[_phaseIndex];
            if (pat.volley == null || pat.volley.Length == 0) { OpenVulnerable(); return; }

            _timer += Time.deltaTime;

            // Fire shots whose delay has elapsed.
            while (_volleySlot < pat.volley.Length &&
                   pat.volley[_volleySlot].delay <= _timer)
            {
                var shot = pat.volley[_volleySlot];
                // Spawn from just in front of the boss, not the far horizon.
                float spawnZ = Config.BossZ + 2f;
                ObstacleSpawner.Instance?.SpawnThreat(shot.input, shot.lane,
                                                       SwipeDirection.None, spawnZ);
                _volleySlot++;
            }

            if (_volleySlot >= pat.volley.Length)
            {
                // Wait a beat after the last shot before opening the window.
                if (_timer >= pat.volley[pat.volley.Length - 1].delay + 0.55f)
                    OpenVulnerable();
            }
        }

        // ── Vulnerable: boss opens weak spot, player must hit it ──────────────
        void OpenVulnerable()
        {
            _phase = Phase.Vulnerable;
            _timer = Config.BossVulnerableWindow;
            ShowWeakSpot(phases[_phaseIndex]);
            PulseBoss(phases[_phaseIndex].weakSpotColor);
        }

        void TickVulnerable()
        {
            if (_weakSpotGo != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.15f;
                _weakSpotGo.transform.localScale = Vector3.one * (0.45f * pulse);
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                // Missed the window — boss shrugs and fires another volley.
                HideWeakSpot();
                BeginVolley();
            }
        }

        // ── input handling ────────────────────────────────────────────────────
        void OnSwipe(SwipeDirection dir)
        {
            if (_phase != Phase.Vulnerable) return;
            if (_phaseIndex >= phases.Length) return;

            var pat = phases[_phaseIndex];
            if (dir != pat.weakSpotInput) return;   // wrong input: ignored (no damage)

            _hitsThisPhase++;
            GameManager.Instance?.AddScore(Config.BossScorePerHit);
            ScorePopup.Show(_weakSpotGo != null ? _weakSpotGo.transform.position
                                                : _bossGo.transform.position,
                            "+" + Config.BossScorePerHit,
                            pat.weakSpotColor);
            CameraShake.Shake(0.2f);
            HitStop.Do(0.04f);

            if (_hitsThisPhase >= pat.hitsToBreak)
                AdvancePhase();
            else
                PulseBoss(Color.white);    // flash hit feedback
        }

        void AdvancePhase()
        {
            HideWeakSpot();
            _phaseIndex++;
            _hitsThisPhase = 0;

            if (_phaseIndex >= phases.Length)
            {
                KillBoss();
                return;
            }

            // Brief recoil pause before the next phase.
            Vfx.Pop(_bossGo.transform.position, phases[_phaseIndex - 1].weakSpotColor);
            CameraShake.Shake(0.4f);
            BeginVolley();
        }

        void KillBoss()
        {
            _phase = Phase.Dead;
            BossActive = false;

            var pos = _bossGo.transform.position;
            for (int i = 0; i < 5; i++)
                Vfx.Pop(pos + Random.insideUnitSphere * 1.2f,
                        new Color(1f, 0.7f, 0.2f));
            CameraShake.Shake(0.8f);
            HitStop.Do(0.08f);

            int bonus = GameManager.Instance.AddScore(Config.BossKillBonus);
            ScorePopup.Show(pos, "+" + bonus, new Color(1f, 0.9f, 0.3f), 1.8f);
            OrbManager.Instance?.TryDropAt(pos, guaranteed: true);

            HideBoss();
            _nextTriggerDist = GameManager.Instance.Distance + Config.BossInterval;
            _phase = Phase.Idle;

            // Resume normal waves: force=true overrides the fight-long freeze.
            SpawnDirector.Instance?.Stun(1.5f, force: true);
        }

        // ── visuals (primitives) ───────────────────────────────────────────────
        void BuildBossVisual()
        {
            if (_bossGo == null)
            {
                _bossGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _bossGo.name = "Boss";
                Destroy(_bossGo.GetComponent<Collider>());
                _bossRend = _bossGo.GetComponent<Renderer>();
            }
            _bossGo.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
            _bossRend.material = MaterialUtil.Colored(new Color(0.18f, 0.18f, 0.22f));
            _bossGo.SetActive(true);
        }

        void ShowWeakSpot(BossPattern pat)
        {
            if (_weakSpotGo == null)
            {
                _weakSpotGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _weakSpotGo.name = "WeakSpot";
                Destroy(_weakSpotGo.GetComponent<Collider>());
                _weakSpotRend = _weakSpotGo.GetComponent<Renderer>();
            }
            _weakSpotGo.transform.position = _bossGo.transform.position + Vector3.up * 0.5f;
            _weakSpotGo.transform.localScale = Vector3.one * 0.45f;
            _weakSpotRend.material = MaterialUtil.Colored(pat.weakSpotColor);
            _weakSpotGo.SetActive(true);
        }

        void HideWeakSpot()
        {
            if (_weakSpotGo != null) _weakSpotGo.SetActive(false);
        }

        void PulseBoss(Color c)
        {
            if (_bossRend != null) _bossRend.material.color = c;
        }

        void HideBoss()
        {
            if (_bossGo != null)    _bossGo.SetActive(false);
            if (_weakSpotGo != null) _weakSpotGo.SetActive(false);
        }

        // ── public read ────────────────────────────────────────────────────────
        /// <summary>0..1 health of the current encounter (for the HUD bar).</summary>
        public float BossHealth01
        {
            get
            {
                if (!BossActive || phases == null || phases.Length == 0) return 0f;
                int totalPhases = phases.Length;
                int totalHits   = totalPhases * Config.BossHitsPerPhase;
                int doneHits    = _phaseIndex * Config.BossHitsPerPhase + _hitsThisPhase;
                return 1f - Mathf.Clamp01((float)doneHits / totalHits);
            }
        }

        public SwipeDirection CurrentWeakSpot =>
            (_phase == Phase.Vulnerable && _phaseIndex < phases.Length)
            ? phases[_phaseIndex].weakSpotInput
            : SwipeDirection.None;
    }
}
