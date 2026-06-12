using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Frost's ultimate — Absolute Zero (GDD §8, §8.1): freezes every threat in place
    /// and holds the spawn for a few seconds while score pays out doubled. Threats
    /// don't advance, so you calmly mop them up at ×2 — the skill/score fantasy.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Ultimates/Freeze", fileName = "FreezeUlt")]
    public class FreezeUltimate : Ultimate
    {
        [Tooltip("How long threats stay frozen and score is multiplied.")]
        public float durationSeconds = 4f;

        [Tooltip("Score multiplier active during the freeze.")]
        [Min(1)] public int scoreMultiplier = 2;

        public override void Activate(in AbilityContext ctx)
        {
            ObstacleSpawner.Instance.FreezeFor(durationSeconds);
            if (SpawnDirector.Instance != null) SpawnDirector.Instance.Stun(durationSeconds);
            GameManager.Instance.SetScoreMultiplier(scoreMultiplier, durationSeconds);
            CameraShake.Shake(0.4f);
            HitStop.Do(0.05f);
        }
    }
}
