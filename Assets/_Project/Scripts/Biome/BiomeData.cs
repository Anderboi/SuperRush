using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// One visual "act" of the run (GDD §11, §2). Holds the palette that BiomeManager
    /// lerps toward once the hero crosses triggerDistance. Create assets via
    /// Create → Ryvok → Biome Data, or let BiomeLibrary build them in code.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        public string biomeName = "Biome";

        [Tooltip("Run distance (m) at which this biome becomes active.")]
        public float triggerDistance = 0f;

        [Header("Palette")]
        public Color skyColor     = new Color(0.42f, 0.27f, 0.66f);
        public Color groundColor  = new Color(0.16f, 0.55f, 0.34f);
        public Color stripeColor  = new Color(0.12f, 0.42f, 0.26f);
        public Color fogColor     = new Color(0.30f, 0.18f, 0.45f);

        [Tooltip("Seconds to cross-fade into this biome's palette.")]
        public float transitionSeconds = 3f;
    }
}
