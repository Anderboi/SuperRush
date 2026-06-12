using UnityEngine;

namespace Ryvok
{
    public enum HeroElement { Electro, Earth, Fire, Ice }
    public enum HeroRarity { Common, Rare, Epic, Legendary }

    /// <summary>
    /// A playable hero as pure data (GDD §8, §14.2). Holds stats and the three things
    /// that make a hero feel distinct without touching the five-input scheme: the
    /// strike flavour, the passive, and the ultimate. Add a hero = author one of these
    /// (plus its ability assets), no gameplay code.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Hero", fileName = "Hero")]
    public class HeroData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Hero";
        public HeroElement element = HeroElement.Electro;
        public HeroRarity rarity = HeroRarity.Common;

        [Header("Stats")]
        [Tooltip("Lives for the run (GDD: Volt/Blaze/Frost = 3, Kremen = 4).")]
        [Min(1)] public int maxLives = 3;

        [Tooltip("Scales the strike zone / input window (GDD: Kremen = 1.2).")]
        [Range(0.5f, 2f)] public float inputWindowMult = 1f;

        [Header("Abilities")]
        public StrikeAbility strike;
        public Passive passive;
        public Ultimate ultimate;
    }
}
