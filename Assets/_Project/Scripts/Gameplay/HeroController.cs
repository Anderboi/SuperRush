using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// The hero stays centred (per the core fantasy: attack, don't dodge). It routes
    /// swipes to the spawner's resolver and plays placeholder squash/flash feedback.
    /// </summary>
    public class HeroController : MonoBehaviour
    {
        Renderer _renderer;
        Color _baseColor;
        Vector3 _baseScale;
        float _punch;   // attack squash timer
        float _flash;   // hurt flash timer

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer != null) _baseColor = _renderer.material.color;
            _baseScale = transform.localScale;
        }

        void OnEnable()
        {
            if (SwipeDetector.Instance != null) SwipeDetector.Instance.OnSwipe += HandleSwipe;
            if (GameManager.Instance != null)  GameManager.Instance.OnHit += OnHit;
        }

        void Start()
        {
            // Re-subscribe safely regardless of Awake ordering.
            if (SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipe -= HandleSwipe;
                SwipeDetector.Instance.OnSwipe += HandleSwipe;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnHit -= OnHit;
                GameManager.Instance.OnHit += OnHit;
            }
        }

        void OnDisable()
        {
            if (SwipeDetector.Instance != null) SwipeDetector.Instance.OnSwipe -= HandleSwipe;
            if (GameManager.Instance != null)  GameManager.Instance.OnHit -= OnHit;
        }

        void HandleSwipe(SwipeDirection dir)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            ObstacleSpawner.Instance.Resolve(dir);
            _punch = 0.12f;
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
