using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Builds the default boss definition in code so the starter runs with zero
    /// assets. Real encounters should be authored as BossData SO assets and
    /// assigned in the BossSystem inspector.
    /// </summary>
    public static class BossLibrary
    {
        /// <summary>
        /// A 3-phase lieutenant of GLITCH (GDD §2): fires volleys of projectiles,
        /// opens a weak spot between them. Each phase is a scripted BossPattern.
        /// </summary>
        public static BossPattern[] DefaultPhases()
        {
            // Phase 1 — simple cross: left then right, weak spot is Tap.
            var p1 = ScriptableObject.CreateInstance<BossPattern>();
            p1.label = "Cross";
            p1.weakSpotInput = SwipeDirection.Tap;
            p1.weakSpotColor = new Color(1f, 0.45f, 0.1f);
            p1.hitsToBreak = Config.BossHitsPerPhase;
            p1.volley = new[]
            {
                new BossPattern.Volley { input = SwipeDirection.Left,  lane = 0, delay = 0f },
                new BossPattern.Volley { input = SwipeDirection.Right, lane = 2, delay = 0.5f },
            };

            // Phase 2 — vertical sweep: drone then ground, weak spot is Left.
            var p2 = ScriptableObject.CreateInstance<BossPattern>();
            p2.label = "Sweep";
            p2.weakSpotInput = SwipeDirection.Left;
            p2.weakSpotColor = new Color(0.3f, 0.6f, 1f);
            p2.hitsToBreak = Config.BossHitsPerPhase;
            p2.volley = new[]
            {
                new BossPattern.Volley { input = SwipeDirection.Up,   lane = 1, delay = 0f   },
                new BossPattern.Volley { input = SwipeDirection.Down, lane = 1, delay = 0.45f },
                new BossPattern.Volley { input = SwipeDirection.Up,   lane = 0, delay = 0.75f },
            };

            // Phase 3 — scramble: 4 shots, weak spot is Up.
            var p3 = ScriptableObject.CreateInstance<BossPattern>();
            p3.label = "Scramble";
            p3.weakSpotInput = SwipeDirection.Up;
            p3.weakSpotColor = new Color(1f, 0.9f, 0.2f);
            p3.hitsToBreak = Config.BossHitsPerPhase;
            p3.volley = new[]
            {
                new BossPattern.Volley { input = SwipeDirection.Left,  lane = 0, delay = 0f   },
                new BossPattern.Volley { input = SwipeDirection.Tap,   lane = 1, delay = 0.35f },
                new BossPattern.Volley { input = SwipeDirection.Right, lane = 2, delay = 0.55f },
                new BossPattern.Volley { input = SwipeDirection.Down,  lane = 1, delay = 0.80f },
            };

            return new[] { p1, p2, p3 };
        }
    }
}
