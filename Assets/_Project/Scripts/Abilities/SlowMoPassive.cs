using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Frost's passive (GDD §8): a "perfect" (early) strike triggers a brief slow-mo,
    /// rewarding reads at the far edge of the strike zone. The skill/score fantasy —
    /// dip into bullet-time, line up the next hit.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Passives/Slow-Mo on Perfect", fileName = "SlowMoPassive")]
    public class SlowMoPassive : Passive
    {
        [Tooltip("Earliness needed to count as perfect (1 = far edge, 0 = hero line).")]
        [Range(0f, 1f)] public float perfectThreshold = 0.6f;

        [Tooltip("Time scale during the slow-mo.")]
        [Range(0.1f, 1f)] public float slowScale = 0.4f;

        [Tooltip("Real-time length of the slow-mo.")]
        public float slowSeconds = 0.25f;

        public override void OnKill(in AbilityContext ctx)
        {
            if (ctx.earliness >= perfectThreshold)
                HitStop.SlowMo(slowScale, slowSeconds);
        }
    }
}
