using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// The effect of a successful directional strike (GDD §6, §8). The spawner has
    /// already removed the threat and scored it; this adds the hero's *feel*:
    /// element-tinted pop, screen shake, hit-stop, and ultimate charge. All five core
    /// inputs share one StrikeAbility per hero, which is why heroes feel different
    /// without changing the control scheme.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Strike Ability", fileName = "Strike")]
    public class StrikeAbility : Ability
    {
        [Tooltip("Element colour of the hit pop (e.g. electric cyan for Volt).")]
        public Color elementTint = new Color(0.55f, 0.92f, 1f);

        [Tooltip("Hit-stop on a kill, in seconds (time freeze for crunch). 0 = none.")]
        public float hitStop = 0.035f;

        [Tooltip("Camera shake trauma added per kill.")]
        public float cameraShake = 0.12f;

        [Tooltip("Ultimate meter filled per kill (meter max lives on the Ultimate).")]
        public float ultChargePerHit = 9f;

        public override void Execute(in AbilityContext ctx)
        {
            Vfx.Pop(ctx.position, elementTint);
            CameraShake.Shake(cameraShake);
            if (hitStop > 0f) HitStop.Do(hitStop);
            ctx.hero.AddCharge(ultChargePerHit);
        }
    }
}
