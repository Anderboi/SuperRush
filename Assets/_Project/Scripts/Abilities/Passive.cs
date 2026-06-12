using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// A hero's always-on trait (GDD §8). Hooks into run events; the base does nothing
    /// so each passive overrides only what it needs. Concrete passives are SO assets.
    /// </summary>
    public abstract class Passive : ScriptableObject
    {
        /// <summary>Called after the hero kills a threat with a directional strike.</summary>
        public virtual void OnKill(in AbilityContext ctx) { }
    }
}
