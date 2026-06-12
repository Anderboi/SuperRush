using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Blaze's ultimate — Inferno (GDD §8, §8.1): for a few seconds the strike zone
    /// becomes a fire sweep that incinerates any threat that enters it. The aggressive
    /// wave-clear payoff.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Ultimates/Burn", fileName = "BurnUlt")]
    public class BurnUltimate : Ultimate
    {
        [Tooltip("Colour of the incineration pops.")]
        public Color color = new Color(1f, 0.5f, 0.15f);

        [Tooltip("How long the fire sweep lasts.")]
        public float durationSeconds = 3f;

        public override void Activate(in AbilityContext ctx)
        {
            ObstacleSpawner.Instance.BurnFor(durationSeconds, color);
            CameraShake.Shake(0.5f);
        }
    }
}
