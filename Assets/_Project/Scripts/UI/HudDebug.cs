using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Throwaway OnGUI HUD so the loop is readable with zero scene setup.
    /// Kept Latin-only to avoid missing-glyph issues in the built-in font.
    /// Replace with a TextMeshPro HUD for production (GDD UI section).
    /// </summary>
    public class HudDebug : MonoBehaviour
    {
        GUIStyle _big, _mid, _small;

        void Init()
        {
            _big = new GUIStyle { fontSize = 44, fontStyle = FontStyle.Bold };
            _big.normal.textColor = Color.white;
            _mid = new GUIStyle { fontSize = 26, fontStyle = FontStyle.Bold };
            _mid.normal.textColor = Color.white;
            _small = new GUIStyle { fontSize = 18 };
            _small.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
        }

        void OnGUI()
        {
            if (_big == null) Init();
            var gm = GameManager.Instance;
            if (gm == null) return;

            switch (gm.State)
            {
                case GameState.Menu:
                    Center("RYVOK", _big, -140);
                    var hc = HeroController.Instance;
                    if (hc != null && hc.Data != null)
                    {
                        Center(hc.Data.displayName + "   (" + hc.Data.rarity + ", " + hc.Data.element + ")", _mid, -64);
                        Center("HP " + hc.Data.maxLives + "   Window x" + hc.Data.inputWindowMult.ToString("0.0")
                               + "   -   Tab to switch hero", _small, -28);
                    }
                    Center("Swipe or press an Arrow / WASD to start", _mid, 14);
                    Center("Up=aerial   Down=ground   Left/Right=lanes   Tap/Space=front   F=ultimate", _small, 56);
                    break;

                case GameState.Playing:
                    GUI.Label(new Rect(20, 16, 400, 60), "Score " + gm.Score, _big);
                    GUI.Label(new Rect(20, 78, 400, 30), "Lives " + gm.Lives, _mid);
                    int combo = ComboManager.Instance != null ? ComboManager.Instance.Combo : 0;
                    if (combo > 1) Center(combo + "x  COMBO", _mid, -Screen.height / 2 + 40);
                    GUI.Label(new Rect(Screen.width - 180, 16, 200, 30), "Speed " + gm.Speed.ToString("0.0"), _small);
                    DrawUltimate();
                    break;

                case GameState.GameOver:
                    Center("GAME OVER", _big, -100);
                    Center("Score " + gm.Score + "    Best " + gm.Best, _mid, -30);
                    Center("Swipe or Space to retry", _small, 20);
                    break;
            }
        }

        // Hero name + ultimate meter / super button (placeholder; real UI is post-MVP).
        void DrawUltimate()
        {
            var hero = HeroController.Instance;
            if (hero == null || hero.Data == null) return;

            GUI.Label(new Rect(Screen.width - 180, 44, 200, 30), hero.Data.displayName, _small);

            const float w = 240f, h = 20f;
            float x = (Screen.width - w) / 2f;
            float y = Screen.height - 64f;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            Color prev = GUI.color;
            GUI.color = hero.UltReady ? new Color(1f, 0.95f, 0.4f) : new Color(0.55f, 0.92f, 1f);
            GUI.Box(new Rect(x + 2f, y + 2f, (w - 4f) * hero.Charge01, h - 4f), GUIContent.none);
            GUI.color = prev;

            if (hero.UltReady)
            {
                if (GUI.Button(new Rect(x, y - 40f, w, 34f), "ULT READY (tap / F)"))
                    SwipeDetector.Instance?.TriggerUltimate();
            }
            else
            {
                GUI.Label(new Rect(x, y - 26f, w, 24f), "ULT " + Mathf.FloorToInt(hero.Charge01 * 100f) + "%", _small);
            }
        }

        void Center(string text, GUIStyle style, float yOffset)
        {
            var size = style.CalcSize(new GUIContent(text));
            GUI.Label(new Rect((Screen.width - size.x) / 2f, Screen.height / 2f + yOffset, size.x, size.y), text, style);
        }
    }
}
