using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Lightweight trauma-based camera shake. Attach to the camera (Bootstrap does
    /// this). Call CameraShake.Shake(amount) from anywhere.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        public float maxOffset = 0.6f;
        public float decay = 1.6f;

        float _trauma;
        Vector3 _base;

        void Awake()
        {
            Instance = this;
            _base = transform.localPosition;
        }

        void LateUpdate()
        {
            if (_trauma <= 0f) { transform.localPosition = _base; return; }
            float shake = _trauma * _trauma;
            Vector3 offset = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                0f) * (maxOffset * shake);
            transform.localPosition = _base + offset;
            _trauma = Mathf.Max(0f, _trauma - decay * Time.deltaTime);
        }

        public static void Shake(float amount)
        {
            if (Instance != null)
                Instance._trauma = Mathf.Clamp01(Instance._trauma + amount);
        }
    }
}
