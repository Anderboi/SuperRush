using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// One phase of a boss fight (GDD §7): a scripted volley of threats fired from
    /// the boss, followed by a vulnerable window where the hero can hit the weak spot.
    ///
    /// Authored as an asset (Create → Ryvok → Boss Pattern) or built in code by
    /// BossLibrary for the zero-setup starter.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Boss Pattern", fileName = "BossPattern")]
    public class BossPattern : ScriptableObject
    {
        [System.Serializable]
        public struct Volley
        {
            [Tooltip("Required input for this projectile.")]
            public SwipeDirection input;

            [Tooltip("Lane override (0–2). For Up/Down which lane it comes from.")]
            [Range(0, 2)] public int lane;

            [Tooltip("Delay in seconds before this shot fires (from start of volley).")]
            [Min(0f)] public float delay;
        }

        [Tooltip("Label for debugging (unused by gameplay).")]
        public string label = "Phase";

        [Tooltip("The projectile barrage the boss fires before opening a weak spot.")]
        public Volley[] volley;

        [Tooltip("The swipe the hero needs to land on the weak spot to deal damage.")]
        public SwipeDirection weakSpotInput;

        [Tooltip("Colour of the weak-spot indicator.")]
        public Color weakSpotColor = new Color(1f, 0.4f, 0.1f);

        [Tooltip("Number of hits on the weak spot to break this phase.")]
        [Min(1)] public int hitsToBreak = 2;
    }
}
