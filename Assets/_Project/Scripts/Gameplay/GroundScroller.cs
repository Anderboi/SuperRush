using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Sells forward motion: a row of stripes scrolls toward the camera at the run
    /// speed and recycles. Pure placeholder — no textures needed.
    /// </summary>
    public class GroundScroller : MonoBehaviour
    {
        public int stripeCount = 24;
        public float spacing = 2f;

        readonly List<Transform> _stripes = new List<Transform>();
        float _length;

        void Start()
        {
            _length = stripeCount * spacing;
            var mat = MaterialUtil.Colored(MaterialUtil.Stripe);
            for (int i = 0; i < stripeCount; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(go.GetComponent<Collider>());
                go.name = "Stripe";
                go.transform.SetParent(transform);
                go.transform.localScale = new Vector3(7f, 0.05f, 0.6f);
                go.transform.position = new Vector3(0f, 0.01f, i * spacing);
                go.GetComponent<Renderer>().material = mat;
                _stripes.Add(go.transform);
            }
        }

        void Update()
        {
            float speed = GameManager.Instance != null && GameManager.Instance.State == GameState.Playing
                ? GameManager.Instance.Speed
                : Config.StartSpeed * 0.5f;

            foreach (Transform s in _stripes)
            {
                Vector3 p = s.position;
                p.z -= speed * Time.deltaTime;
                if (p.z < -4f) p.z += _length;
                s.position = p;
            }
        }
    }
}
