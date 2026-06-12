using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Default biome palette library (GDD §11). Built purely in code so the game
    /// runs with zero asset setup. Author your own BiomeData assets and assign them
    /// to BiomeManager.biomes to override these.
    ///
    ///  0 m  — Streets    (neon-green / deep purple)
    ///  500 m — Highway   (amber asphalt / night blue)
    /// 1000 m — Rooftops  (slate / cyan dusk)
    /// 1500 m — Subway    (dark concrete / red-orange neon)
    /// 2000 m — Core      (black / glitch-white — final stretch)
    /// </summary>
    public static class BiomeLibrary
    {
        public static BiomeData[] Default()
        {
            return new[]
            {
                Make("Streets",
                    triggerDist:    0f,
                    transitionSec:  0f,   // immediate — starting palette
                    sky:     new Color(0.42f, 0.27f, 0.66f),
                    ground:  new Color(0.16f, 0.55f, 0.34f),
                    stripe:  new Color(0.12f, 0.42f, 0.26f),
                    fog:     new Color(0.30f, 0.18f, 0.45f)),

                Make("Highway",
                    triggerDist:   500f,
                    transitionSec:  3.5f,
                    sky:     new Color(0.08f, 0.10f, 0.22f),
                    ground:  new Color(0.28f, 0.26f, 0.22f),
                    stripe:  new Color(0.85f, 0.55f, 0.10f),
                    fog:     new Color(0.10f, 0.12f, 0.22f)),

                Make("Rooftops",
                    triggerDist:  1000f,
                    transitionSec:  3f,
                    sky:     new Color(0.14f, 0.24f, 0.38f),
                    ground:  new Color(0.30f, 0.32f, 0.35f),
                    stripe:  new Color(0.20f, 0.75f, 0.82f),
                    fog:     new Color(0.12f, 0.22f, 0.30f)),

                Make("Subway",
                    triggerDist:  1500f,
                    transitionSec:  3f,
                    sky:     new Color(0.08f, 0.07f, 0.10f),
                    ground:  new Color(0.20f, 0.18f, 0.20f),
                    stripe:  new Color(0.95f, 0.30f, 0.15f),
                    fog:     new Color(0.12f, 0.05f, 0.08f)),

                Make("Core Approach",
                    triggerDist:  2000f,
                    transitionSec:  4f,
                    sky:     new Color(0.04f, 0.04f, 0.06f),
                    ground:  new Color(0.10f, 0.10f, 0.12f),
                    stripe:  new Color(0.90f, 0.95f, 1.00f),
                    fog:     new Color(0.06f, 0.06f, 0.10f)),
            };
        }

        static BiomeData Make(string name, float triggerDist, float transitionSec,
                              Color sky, Color ground, Color stripe, Color fog)
        {
            var b = ScriptableObject.CreateInstance<BiomeData>();
            b.name            = name;
            b.biomeName       = name;
            b.triggerDistance = triggerDist;
            b.transitionSeconds = transitionSec;
            b.skyColor        = sky;
            b.groundColor     = ground;
            b.stripeColor     = stripe;
            b.fogColor        = fog;
            return b;
        }
    }
}
