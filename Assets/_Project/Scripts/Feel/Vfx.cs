using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Placeholder hit-pop. Spawns a short-lived expanding sphere. For production,
    /// replace with a pooled ParticleSystem (GDD Feel/VFXPool).
    /// </summary>
    public static class Vfx
    {
        public static void Pop(Vector3 pos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.4f;
            go.GetComponent<Renderer>().material = MaterialUtil.Colored(color);
            go.AddComponent<PopFx>();
        }
    }

    public class PopFx : MonoBehaviour
    {
        float _life = 0.28f;
        float _age;
        Vector3 _from = Vector3.one * 0.4f;
        Vector3 _to = Vector3.one * 1.8f;

        void Update()
        {
            _age += Time.deltaTime;
            float t = _age / _life;
            transform.localScale = Vector3.Lerp(_from, _to, t);
            if (t >= 1f) Destroy(gameObject);
        }
    }
}
