using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Floating "+N" score numbers at the hit position (GDD §3: juice on every
    /// action). World-space TextMesh that rises, scales and fades, billboarded to the
    /// camera. Placeholder like Vfx — pool it for production (GDD Feel/VFXPool).
    /// </summary>
    public static class ScorePopup
    {
        static Font _font;

        public static void Show(Vector3 pos, string text, Color color, float size = 1f)
        {
            if (_font == null)
            {
                // Unity 2022.2+ ships LegacyRuntime; older versions ship Arial.
                _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (_font == null) return;
            }

            var go = new GameObject("ScorePopup");
            go.transform.position = pos + Vector3.up * 0.6f;

            var tm = go.AddComponent<TextMesh>();
            tm.font = _font;
            tm.text = text;
            tm.color = color;
            tm.fontSize = 56;
            tm.characterSize = 0.045f * size;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.fontStyle = FontStyle.Bold;
            go.GetComponent<MeshRenderer>().material = _font.material;

            go.AddComponent<PopupFx>();
        }
    }

    public class PopupFx : MonoBehaviour
    {
        const float Life = 0.7f;
        float _age;
        TextMesh _tm;
        Color _color;

        void Awake()
        {
            _tm = GetComponent<TextMesh>();
        }

        void Start()
        {
            _color = _tm.color;
        }

        void LateUpdate()
        {
            _age += Time.deltaTime;
            float t = _age / Life;

            // Rise, slight pop-in scale, fade out at the end.
            transform.position += Vector3.up * (1.6f * Time.deltaTime);
            float scale = 1f + 0.35f * Mathf.Sin(Mathf.Min(t * 3f, 1f) * Mathf.PI * 0.5f);
            transform.localScale = Vector3.one * scale;

            if (Camera.main != null)
                transform.rotation = Camera.main.transform.rotation;

            _tm.color = new Color(_color.r, _color.g, _color.b, _color.a * Mathf.Clamp01(2f - t * 2f));

            if (t >= 1f) Destroy(gameObject);
        }
    }
}
