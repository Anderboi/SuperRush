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
        public int Coins { get; private set; }   // this run's pickups; banked into the save on death
        public float Distance { get; private set; }
        public int Best { get; private set; }
        public float BestDistance { get; private set; }
        public bool NewBest { get; private set; }   // this run set a score record (results flair)

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
            Best = SaveSystem.Data.bestScore;
            BestDistance = SaveSystem.Data.bestDistance;
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
            Coins = 0;
            Lives = HeroController.Instance != null ? HeroController.Instance.MaxLives : Config.StartLives;
            Distance = 0f;
            Speed = Config.StartSpeed;
            _scoreMultUntil = 0f;
            SetState(GameState.Playing);
            OnRunStart?.Invoke();
        }

        float _scoreMultUntil;
        int _scoreMultFactor = 1;

        /// <summary>Apply a temporary score multiplier (Frost ult ×2, GDD §8).</summary>
        public void SetScoreMultiplier(int factor, float seconds)
        {
            _scoreMultFactor = Mathf.Max(1, factor);
            _scoreMultUntil = Time.time + seconds;
        }

        /// <summary>Add score. Returns the final amount (after any active multiplier).</summary>
        public int AddScore(int amount)
        {
            if (amount == 0) return 0;
            if (Time.time < _scoreMultUntil) amount *= _scoreMultFactor;
            Score += amount;
            OnScored?.Invoke(amount);
            return amount;
        }

        public void AddCoins(int amount) => Coins += amount;

        public void Damage()
        {
            if (State != GameState.Playing) return;
            Lives--;
            OnHit?.Invoke();
            if (Lives <= 0) EndRun();
        }

        void EndRun()
        {
            NewBest = Score > Best;
            Best = Mathf.Max(Best, Score);
            BestDistance = Mathf.Max(BestDistance, Distance);

            // Persist records + bank the run's coins (GDD §14.2 Save System).
            var save = SaveSystem.Data;
            save.bestScore = Best;
            save.bestDistance = BestDistance;
            save.coins += Coins;
            save.runs++;
            SaveSystem.Save();

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
