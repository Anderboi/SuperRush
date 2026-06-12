using System;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Owns run state: game state, score, lives, distance, and the speed ramp.
    /// Other systems read Speed and listen to the events.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Menu;
        public float Speed { get; private set; }
        public int Lives { get; private set; }
        public int Score { get; private set; }
        public float Distance { get; private set; }
        public int Best { get; private set; }

        public event Action OnRunStart;
        public event Action OnRunEnd;
        public event Action OnStateChanged;
        public event Action OnHit;            // hero took damage
        public event Action<int> OnScored;    // amount added

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Speed = Config.StartSpeed;
        }

        void OnEnable()
        {
            if (SwipeDetector.Instance != null)
                SwipeDetector.Instance.OnSwipe += HandleSwipe;
        }

        void Start()
        {
            // Subscribe late in case SwipeDetector.Awake ran after ours.
            if (SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipe -= HandleSwipe;
                SwipeDetector.Instance.OnSwipe += HandleSwipe;
            }
        }

        void OnDisable()
        {
            if (SwipeDetector.Instance != null)
                SwipeDetector.Instance.OnSwipe -= HandleSwipe;
        }

        // Menu / GameOver: any input starts or restarts the run.
        void HandleSwipe(SwipeDirection dir)
        {
            if (State == GameState.Menu || State == GameState.GameOver)
                StartRun();
        }

        float _distCarry;

        void Update()
        {
            if (State != GameState.Playing) return;

            Speed = Mathf.Min(Config.MaxSpeed, Speed + Config.SpeedRampPerSec * Time.deltaTime);
            float step = Speed * Time.deltaTime;
            Distance += step;

            // Distance trickle: +1 score per 2 units travelled.
            _distCarry += step;
            while (_distCarry >= 2f) { _distCarry -= 2f; AddScore(1); }
        }

        public void StartRun()
        {
            Score = 0;
            Lives = HeroController.Instance != null ? HeroController.Instance.MaxLives : Config.StartLives;
            Distance = 0f;
            Speed = Config.StartSpeed;
            SetState(GameState.Playing);
            OnRunStart?.Invoke();
        }

        public void AddScore(int amount)
        {
            if (amount != 0)
            {
                Score += amount;
                OnScored?.Invoke(amount);
            }
        }

        public void Damage()
        {
            if (State != GameState.Playing) return;
            Lives--;
            OnHit?.Invoke();
            if (Lives <= 0) EndRun();
        }

        void EndRun()
        {
            Best = Mathf.Max(Best, Score);
            SetState(GameState.GameOver);
            OnRunEnd?.Invoke();
        }

        void SetState(GameState s)
        {
            State = s;
            OnStateChanged?.Invoke();
        }
    }
}
