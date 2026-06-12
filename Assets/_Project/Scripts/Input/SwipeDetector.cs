using System;
using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Detects the five core inputs from touch, mouse (editor), or keyboard.
    /// Raises OnSwipe with a SwipeDirection. Uses legacy Input for zero-setup
    /// prototyping — production should move to the Input System (GDD 14.1) behind
    /// this same OnSwipe interface, so nothing else needs to change.
    /// </summary>
    public class SwipeDetector : MonoBehaviour
    {
        public static SwipeDetector Instance { get; private set; }

        [Tooltip("Minimum screen-pixel travel to count as a directional swipe (else it's a tap).")]
        public float swipeThreshold = 40f;

        public event Action<SwipeDirection> OnSwipe;

        /// <summary>Ultimate trigger — a separate input from the five combat swipes.</summary>
        public event Action OnUltimate;

        Vector2 _startPos;
        bool _tracking;

        /// <summary>Raise the ultimate input (keyboard F or the HUD super button).</summary>
        public void TriggerUltimate() => OnUltimate?.Invoke();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Update()
        {
            HandleKeyboard();
            HandleTouch();
            HandleMouse();
        }

        void HandleKeyboard()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)    || Input.GetKeyDown(KeyCode.W)) Emit(SwipeDirection.Up);
            if (Input.GetKeyDown(KeyCode.DownArrow)  || Input.GetKeyDown(KeyCode.S)) Emit(SwipeDirection.Down);
            if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) Emit(SwipeDirection.Left);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) Emit(SwipeDirection.Right);
            if (Input.GetKeyDown(KeyCode.Space)      || Input.GetKeyDown(KeyCode.Return)) Emit(SwipeDirection.Tap);
            if (Input.GetKeyDown(KeyCode.F)) TriggerUltimate();
        }

        void HandleTouch()
        {
            if (Input.touchCount == 0) return;
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began) { _startPos = t.position; _tracking = true; }
            else if (t.phase == TouchPhase.Ended && _tracking)
            {
                Resolve(t.position - _startPos);
                _tracking = false;
            }
        }

        void HandleMouse()
        {
            // Mouse fallback for editor testing without a touchscreen.
            if (Input.touchSupported) return;
            if (Input.GetMouseButtonDown(0)) { _startPos = Input.mousePosition; _tracking = true; }
            else if (Input.GetMouseButtonUp(0) && _tracking)
            {
                Resolve((Vector2)Input.mousePosition - _startPos);
                _tracking = false;
            }
        }

        void Resolve(Vector2 delta)
        {
            if (delta.magnitude < swipeThreshold) { Emit(SwipeDirection.Tap); return; }
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                Emit(delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);
            else
                Emit(delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left);
        }

        void Emit(SwipeDirection dir) => OnSwipe?.Invoke(dir);
    }
}
