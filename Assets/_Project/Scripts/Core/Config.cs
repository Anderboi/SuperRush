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
    }

    public enum HeightLevel { Low, Mid, High }
}
