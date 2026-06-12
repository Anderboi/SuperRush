using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Volt's ultimate — Thunderstorm (GDD §8): clears every threat on screen as kills
    /// and stuns the spawn for a beat (no new threats while the storm settles).
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Ultimates/Screen Clear", fileName = "ScreenClearUlt")]
    public class ScreenClearUltimate : Ultimate
    {
        [Tooltip("Colour of the clear-out pops.")]
        public Color color = new Color(0.6f, 0.9f, 1f);

        [Tooltip("Seconds the Spawn Director is held off after the blast (the 'stun').")]
        public float stunSeconds = 1.6f;

        public override void Activate(in AbilityContext ctx)
        {
            ObstacleSpawner.Instance.ClearAllAsKills(color);
            if (SpawnDirector.Instance != null) SpawnDirector.Instance.Stun(stunSeconds);
            CameraShake.Shake(0.7f);
            HitStop.Do(0.07f);
        }
    }
}
