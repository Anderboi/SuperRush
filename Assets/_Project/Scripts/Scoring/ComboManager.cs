using System;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Tracks the kill streak and its decay window. Multiplier grows every 5 kills.
    /// </summary>
    public class ComboManager : MonoBehaviour
    {
        public static ComboManager Instance { get; private set; }

        public int Combo { get; private set; }
        public int Multiplier => 1 + Combo / 5;

        public event Action OnComboChanged;

        float _timer;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart += ResetCombo;
                GameManager.Instance.OnHit += ResetCombo;
            }
        }

        void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= ResetCombo;
                GameManager.Instance.OnHit -= ResetCombo;
            }
        }

        void Start()
        {
            // Re-subscribe safely regardless of Awake ordering (manual scene setups).
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRunStart -= ResetCombo;
                GameManager.Instance.OnHit -= ResetCombo;
                GameManager.Instance.OnRunStart += ResetCombo;
                GameManager.Instance.OnHit += ResetCombo;
            }
        }

        void Update()
        {
            if (Combo <= 0) return;
            _timer -= Time.deltaTime;
            if (_timer <= 0f) ResetCombo();
        }

        public void RegisterKill()
        {
            Combo++;
            _timer = Config.ComboWindow;
            OnComboChanged?.Invoke();
        }

        public void Whiff()
        {
            if (Combo > 0) { Combo--; OnComboChanged?.Invoke(); }
        }

        public void ResetCombo()
        {
            if (Combo == 0) return;
            Combo = 0;
            OnComboChanged?.Invoke();
        }
    }
}
