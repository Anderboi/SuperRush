using System.Collections.Generic;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Builds the hero roster in code so the starter is playable without authoring
    /// assets. In production these would be SO assets edited in the inspector; the
    /// runtime shape is identical. Roles/stats follow the balance board (GDD §8.1).
    /// </summary>
    public static class HeroLibrary
    {
        /// <summary>The full starter roster (GDD §8): Volt, Blaze, Kremen, Frost.</summary>
        public static List<HeroData> Roster() =>
            new List<HeroData> { Volt(), Blaze(), Kremen(), Frost() };

        /// <summary>Volt — Common, electro, combo/balance baseline.</summary>
        public static HeroData Volt()
        {
            var strike = Strike("Volt Strike", new Color(0.55f, 0.92f, 1f), 0.035f, 9f);

            var passive = ScriptableObject.CreateInstance<ChainPassive>();
            passive.name = "Chain Lightning";
            passive.everyNthHit = 5;
            passive.chainColor = new Color(0.8f, 0.97f, 1f);

            var ult = ScriptableObject.CreateInstance<ScreenClearUltimate>();
            ult.name = "Thunderstorm";
            ult.chargeMax = 100f;
            ult.color = new Color(0.6f, 0.9f, 1f);
            ult.stunSeconds = 1.6f;

            return Hero("Volt", "VOLT", HeroElement.Electro, HeroRarity.Common,
                        maxLives: 3, window: 1f, strike, passive, ult);
        }

        /// <summary>Blaze — Rare, fire, multikill / wave-clear (aggressive).</summary>
        public static HeroData Blaze()
        {
            var strike = Strike("Blaze Strike", new Color(1f, 0.55f, 0.2f), 0.03f, 9f);

            // Fire spreads to the next enemy on every kill (chain with N=1).
            var passive = ScriptableObject.CreateInstance<ChainPassive>();
            passive.name = "Spreading Fire";
            passive.everyNthHit = 1;
            passive.chainColor = new Color(1f, 0.6f, 0.25f);

            var ult = ScriptableObject.CreateInstance<BurnUltimate>();
            ult.name = "Inferno";
            ult.chargeMax = 100f;
            ult.color = new Color(1f, 0.5f, 0.15f);
            ult.durationSeconds = 3f;

            return Hero("Blaze", "BLAZE", HeroElement.Fire, HeroRarity.Rare,
                        maxLives: 3, window: 1f, strike, passive, ult);
        }

        /// <summary>Kremen — Epic, earth, tank / survivability (4 HP, +20% window).</summary>
        public static HeroData Kremen()
        {
            var strike = Strike("Kremen Strike", new Color(0.7f, 0.55f, 0.35f), 0.05f, 8f);

            // His "passive" is the stat line (4 HP + wider window) — no behaviour SO.
            var ult = ScriptableObject.CreateInstance<ScreenClearUltimate>();
            ult.name = "Rockslide";
            ult.chargeMax = 130f;          // slow to charge (GDD §8.1)
            ult.color = new Color(0.72f, 0.58f, 0.38f);
            ult.stunSeconds = 2.2f;        // heavier payoff

            return Hero("Kremen", "KREMEN", HeroElement.Earth, HeroRarity.Epic,
                        maxLives: 4, window: 1.2f, strike, passive: null, ult);
        }

        /// <summary>Frost — Legendary, ice, skill / score / records.</summary>
        public static HeroData Frost()
        {
            var strike = Strike("Frost Strike", new Color(0.6f, 0.92f, 1f), 0.035f, 9f);

            var passive = ScriptableObject.CreateInstance<SlowMoPassive>();
            passive.name = "Cold Focus";
            passive.perfectThreshold = 0.6f;
            passive.slowScale = 0.4f;
            passive.slowSeconds = 0.25f;

            var ult = ScriptableObject.CreateInstance<FreezeUltimate>();
            ult.name = "Absolute Zero";
            ult.chargeMax = 110f;
            ult.durationSeconds = 4f;
            ult.scoreMultiplier = 2;

            return Hero("Frost", "FROST", HeroElement.Ice, HeroRarity.Legendary,
                        maxLives: 3, window: 1f, strike, passive, ult);
        }

        // ---- builders ----
        static StrikeAbility Strike(string name, Color tint, float hitStop, float charge)
        {
            var s = ScriptableObject.CreateInstance<StrikeAbility>();
            s.name = name;
            s.elementTint = tint;
            s.hitStop = hitStop;
            s.cameraShake = 0.12f;
            s.ultChargePerHit = charge;
            return s;
        }

        static HeroData Hero(string assetName, string display, HeroElement element, HeroRarity rarity,
                             int maxLives, float window, StrikeAbility strike, Passive passive, Ultimate ult)
        {
            var hero = ScriptableObject.CreateInstance<HeroData>();
            hero.name = assetName;
            hero.displayName = display;
            hero.element = element;
            hero.rarity = rarity;
            hero.maxLives = maxLives;
            hero.inputWindowMult = window;
            hero.strike = strike;
            hero.passive = passive;
            hero.ultimate = ult;
            return hero;
        }
    }
}
