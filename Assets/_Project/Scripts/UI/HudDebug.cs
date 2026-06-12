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
                    Center("RYVOK", _big, -120);
                    Center("Swipe or press an Arrow / WASD to start", _mid, -40);
                    Center("Up=aerial   Down=ground   Left/Right=lanes   Tap/Space=front", _small, 10);
                    break;

                case GameState.Playing:
                    GUI.Label(new Rect(20, 16, 400, 60), "Score " + gm.Score, _big);
                    GUI.Label(new Rect(20, 78, 400, 30), "Lives " + gm.Lives, _mid);
                    int combo = ComboManager.Instance != null ? ComboManager.Instance.Combo : 0;
                    if (combo > 1) Center(combo + "x  COMBO", _mid, -Screen.height / 2 + 40);
                    GUI.Label(new Rect(Screen.width - 180, 16, 200, 30), "Speed " + gm.Speed.ToString("0.0"), _small);
                    break;

                case GameState.GameOver:
                    Center("GAME OVER", _big, -100);
                    Center("Score " + gm.Score + "    Best " + gm.Best, _mid, -30);
                    Center("Swipe or Space to retry", _small, 20);
                    break;
            }
        }

        void Center(string text, GUIStyle style, float yOffset)
        {
            var size = style.CalcSize(new GUIContent(text));
            GUI.Label(new Rect((Screen.width - size.x) / 2f, Screen.height / 2f + yOffset, size.x, size.y), text, style);
        }
    }
}
