using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// A hero's ultimate (GDD §8). Charges 0→<see cref="chargeMax"/> from kills (and
    /// orbs later); the player taps the super button to fire it. Concrete ultimates
    /// are SO assets.
    /// </summary>
    public abstract class Ultimate : ScriptableObject
    {
        [Tooltip("Charge needed to fire (the meter fills from strikes).")]
        [Min(1f)] public float chargeMax = 100f;

        public abstract void Activate(in AbilityContext ctx);
    }
}
