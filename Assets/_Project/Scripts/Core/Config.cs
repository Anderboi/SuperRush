using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Central tuning constants for the playable core. Keep gameplay numbers here
    /// so spawner / hero / resolver never disagree on lane and height positions.
    /// </summary>
    public static class Config
    {
        // Lanes along X (left / center / right).
        public static readonly float[] LaneX = { -2f, 0f, 2f };
        public const int LaneLeft = 0;
        public const int LaneCenter = 1;
        public const int LaneRight = 2;

        // Heights along Y (low / mid / high), relative to ground at y = 0.
        public const float HeightLow = 0.5f;
        public const float HeightMid = 1.2f;
        public const float HeightHigh = 2.4f;

        // Forward axis: obstacles spawn at +Z and move toward the hero at z = 0.
        public const float SpawnZ = 42f;
        public const float HeroZ = 0f;

        // A swipe can resolve any obstacle whose z is within this band ahead of the hero.
        public const float StrikeZoneMin = -0.5f;
        public const float StrikeZoneMax = 14f;

        // Speed / difficulty ramp.
        public const float StartSpeed = 9f;
        public const float MaxSpeed = 20f;
        public const float SpeedRampPerSec = 0.06f;   // +units/sec of speed, per second

        // Spawn gap (seconds) shrinks as the run gets faster. The Spawn Director
        // treats this gap as one "beat" and scales authored pattern spacing by it.
        public const float StartGap = 1.5f;
        public const float MinGap = 0.55f;

        // Rest between authored patterns, in beats (see SpawnDirector).
        public const float PatternRestBeats = 1.0f;

        // Run rules.
        public const int StartLives = 3;
        public const int ScorePerKill = 10;
        public const float ComboWindow = 2.5f;

        // Narrative meta-goal (GDD §2): the Core sits this far away; best distance
        // maps to "how far the team has pushed" on the progress readout.
        public const float CoreDistance = 2500f;

        // Boss encounters (GDD §7: every ~500 m, multi-phase, scripted patterns).
        public const float BossInterval = 500f;     // metres between lieutenants
        public const float BossZ = 20f;             // where the boss parks ahead of the hero
        public const int   BossHitsPerPhase = 2;    // weak-spot hits to break one phase
        public const float BossVulnerableWindow = 1.6f;  // seconds per weak-spot read
        public const int   BossScorePerHit = 25;
        public const int   BossKillBonus = 250;

        // Two-step obstacles (GDD §7.1).
        public const float StaggerWindow = 0.6f;    // seconds to land the 2nd input
        public const int   TwoStepBonus  = 15;      // bonus score on a finished two-step (×combo)
        public const float TwoStepScale  = 1.25f;   // bigger silhouette so it reads as special

        // Drops (GDD §7, §9): some kills shed energy orbs that home to the hero.
        public const float OrbDropChance  = 0.35f;  // chance a kill drops orbs (two-step always does)
        public const int   OrbsPerDrop    = 3;
        public const int   OrbScore       = 5;      // score per collected orb (GDD §9)
        public const float OrbCharge      = 3f;     // ult charge per collected orb
        public const float CoinDropChance = 0.12f;  // extra roll: a coin alongside the orbs
    }

    public enum HeightLevel { Low, Mid, High }
}
