using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Builds hero data in code so the zero-setup starter has a playable hero without
    /// authoring assets. In production these would be SpawnPattern-style SO assets
    /// edited in the inspector; the runtime shape is identical.
    /// </summary>
    public static class HeroLibrary
    {
        /// <summary>Volt — Common, electro, combo/balance baseline (GDD §8, §8.1).</summary>
        public static HeroData Volt()
        {
            var strike = ScriptableObject.CreateInstance<StrikeAbility>();
            strike.name = "Volt Strike";
            strike.elementTint = new Color(0.55f, 0.92f, 1f);
            strike.hitStop = 0.035f;
            strike.cameraShake = 0.12f;
            strike.ultChargePerHit = 9f;

            var passive = ScriptableObject.CreateInstance<ChainPassive>();
            passive.name = "Chain Lightning";
            passive.everyNthHit = 5;
            passive.chainColor = new Color(0.8f, 0.97f, 1f);

            var ult = ScriptableObject.CreateInstance<ScreenClearUltimate>();
            ult.name = "Thunderstorm";
            ult.chargeMax = 100f;
            ult.color = new Color(0.6f, 0.9f, 1f);
            ult.stunSeconds = 1.6f;

            var hero = ScriptableObject.CreateInstance<HeroData>();
            hero.name = "Volt";
            hero.displayName = "VOLT";
            hero.element = HeroElement.Electro;
            hero.rarity = HeroRarity.Common;
            hero.maxLives = 3;
            hero.inputWindowMult = 1f;
            hero.strike = strike;
            hero.passive = passive;
            hero.ultimate = ult;
            return hero;
        }
    }
}
