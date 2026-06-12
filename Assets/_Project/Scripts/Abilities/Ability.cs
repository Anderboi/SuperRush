using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Data passed to any ability when it fires: who cast it, the input that triggered
    /// it, and where it happened (for VFX). Abilities reach the field via the
    /// ObstacleSpawner singleton.
    /// </summary>
    public struct AbilityContext
    {
        public HeroController hero;
        public SwipeDirection input;
        public Vector3 position;
    }

    /// <summary>
    /// Base type for the input→effect mapping (GDD §14.2). A hero's five directional
    /// inputs resolve a threat (universal rule) and then run the hero's ability for
    /// flavor: element VFX, hit-stop, ult charge. Authored as ScriptableObject assets
    /// so heroes are data, not code.
    /// </summary>
    public abstract class Ability : ScriptableObject
    {
        public abstract void Execute(in AbilityContext ctx);
    }
}
