using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// An authored wave — a deliberate "chunk" of threats with a fixed shape and
    /// rhythm (GDD §9, §14.2). The Spawn Director picks patterns whose difficulty
    /// band matches the current run and plays their slots in order, so the field
    /// reads as designer waves instead of random mush.
    ///
    /// Author these as assets in the editor (Create → Ryvok → Spawn Pattern). The
    /// starter also builds a default library in code (SpawnDirector.BuildDefaultLibrary)
    /// so the zero-setup Play-button flow works without any assets.
    /// </summary>
    [CreateAssetMenu(menuName = "Ryvok/Spawn Pattern", fileName = "Pattern")]
    public class SpawnPattern : ScriptableObject
    {
        [System.Serializable]
        public struct Slot
        {
            [Tooltip("Which threat / required (first) input (per GDD §6–7).")]
            public SwipeDirection input;

            [Tooltip("Lane for Up/Down threats (0=left, 1=center, 2=right). " +
                     "Ignored for Left/Right/Tap, which force their own lane.")]
            [Range(0, 2)] public int lane;

            [Tooltip("Spacing before this slot, in beats. The director scales one beat " +
                     "to the current difficulty gap, so authored patterns tighten as " +
                     "speed rises instead of falling apart.")]
            public float beats;

            [Tooltip("Second input for a two-step obstacle (GDD §7.1). None = single swipe.")]
            public SwipeDirection second;

            public Slot(SwipeDirection input, int lane, float beats, SwipeDirection second = SwipeDirection.None)
            {
                this.input = input;
                this.lane = lane;
                this.beats = beats;
                this.second = second;
            }
        }

        [Tooltip("Designer label — shown in logs, ignored by gameplay.")]
        public string label = "Pattern";

        [Tooltip("Difficulty band [0..1] in which this pattern is eligible. " +
                 "Difficulty = how far the run's speed has ramped from start to max.")]
        [Range(0f, 1f)] public float minDifficulty = 0f;
        [Range(0f, 1f)] public float maxDifficulty = 1f;

        [Tooltip("Threats in order.")]
        public Slot[] slots;

        public bool FitsDifficulty(float d) => d >= minDifficulty && d <= maxDifficulty;
        public bool IsPlayable => slots != null && slots.Length > 0;
    }
}
