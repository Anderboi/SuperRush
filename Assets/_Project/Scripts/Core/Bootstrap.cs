using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// One-click scene builder. Put this component on a single empty GameObject in
    /// an otherwise empty scene and press Play — it creates the camera, light,
    /// ground, hero and all systems from primitives. No prefabs or assets required.
    ///
    /// This is a prototype convenience. For production you'd author the scene in the
    /// editor and wire components in the inspector (GDD 14.5).
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            BuildCamera();
            BuildLight();
            BuildGround();
            BuildHero();
            BuildSystems();
        }

        void BuildCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }
            cam.transform.position = new Vector3(0f, 3.4f, -6.8f);
            cam.transform.LookAt(new Vector3(0f, 1.1f, 7f));
            cam.fieldOfView = 62f;
            cam.backgroundColor = new Color(0.42f, 0.27f, 0.66f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            if (cam.GetComponent<CameraShake>() == null) cam.gameObject.AddComponent<CameraShake>();
        }

        void BuildLight()
        {
            if (FindObjectOfType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(1.6f, 1f, 9f);
            ground.transform.position = new Vector3(0f, 0f, 30f);
            ground.GetComponent<Renderer>().material = MaterialUtil.Colored(MaterialUtil.Ground);
        }

        void BuildHero()
        {
            var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = "Hero";
            hero.transform.position = new Vector3(0f, 1f, 0f);
            hero.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            hero.GetComponent<Renderer>().material = MaterialUtil.Colored(MaterialUtil.Hero);
            hero.AddComponent<HeroController>();   // builds the roster; defaults to Volt, Tab to switch
        }

        void BuildSystems()
        {
            var sys = new GameObject("_Systems");
            // Order matters a little: input first so managers can find it on Awake.
            sys.AddComponent<SwipeDetector>();
            sys.AddComponent<GameManager>();
            sys.AddComponent<ComboManager>();
            sys.AddComponent<ObstacleSpawner>();   // the field (pool + advance + resolve)
            sys.AddComponent<SpawnDirector>();     // decides what/when to spawn
            sys.AddComponent<OrbManager>();        // pickup drops (orbs/coins)
            sys.AddComponent<GroundScroller>();
            sys.AddComponent<HitStop>();
            sys.AddComponent<HudDebug>();
        }
    }
}
