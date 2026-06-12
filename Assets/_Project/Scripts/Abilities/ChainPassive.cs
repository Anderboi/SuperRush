using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Volt's passive (GDD §8): every Nth strike arcs to a neighbouring threat in the
    /// strike zone and destroys it too. Rewards keeping a streak alive.
    ///
    /// The "every Nth" counter is run state on the hero (HeroController.StrikeCount),
    /// not on this shared asset.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Passives/Chain", fileName = "ChainPassive")]
    public class ChainPassive : Passive
    {
        [Tooltip("Arc fires on every Nth successful strike.")]
        [Min(2)] public int everyNthHit = 5;

        [Tooltip("Colour of the chained-arc pop.")]
        public Color chainColor = new Color(0.8f, 0.97f, 1f);

        public override void OnKill(in AbilityContext ctx)
        {
            if (ctx.hero.StrikeCount % everyNthHit != 0) return;
            ObstacleSpawner.Instance.TryChainKill(ctx.position, chainColor);
        }
    }
}
