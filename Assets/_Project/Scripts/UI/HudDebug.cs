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
        GUIStyle _big, _mid, _small, _accent;

        void Init()
        {
            _big = new GUIStyle { fontSize = 44, fontStyle = FontStyle.Bold };
            _big.normal.textColor = Color.white;
            _mid = new GUIStyle { fontSize = 26, fontStyle = FontStyle.Bold };
            _mid.normal.textColor = Color.white;
            _small = new GUIStyle { fontSize = 18 };
            _small.normal.textColor = new Color(1f, 1f, 1f, 0.85f);
            _accent = new GUIStyle { fontSize = 26, fontStyle = FontStyle.Bold };
            _accent.normal.textColor = new Color(1f, 0.9f, 0.3f);
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
                    DrawCoreProgress(96);
                    Center("Best " + gm.Best + "    Bank " + SaveSystem.Data.coins + " coins"
                           + "    Runs " + SaveSystem.Data.runs, _small, 146);
                    break;

                case GameState.Playing:
                    GUI.Label(new Rect(20, 16, 400, 60), "Score " + gm.Score, _big);
                    GUI.Label(new Rect(20, 78, 400, 30), "Lives " + gm.Lives, _mid);
                    GUI.Label(new Rect(20, 112, 400, 26), "Coins " + gm.Coins, _small);
                    int combo = ComboManager.Instance != null ? ComboManager.Instance.Combo : 0;
                    if (combo > 1) Center(combo + "x  COMBO", _mid, -Screen.height / 2 + 40);
                    GUI.Label(new Rect(Screen.width - 180, 16, 200, 30), "Speed " + gm.Speed.ToString("0.0"), _small);
                    var bm = BiomeManager.Instance;
                    if (bm != null && bm.CurrentBiomeName.Length > 0)
                        GUI.Label(new Rect(Screen.width - 180, 44, 200, 26),
                                  bm.CurrentBiomeName + (bm.InTransition ? "..." : ""), _small);
                    DrawUltimate();
                    DrawBossBar();
                    break;

                case GameState.GameOver:
                    Center("GAME OVER", _big, -150);
                    if (gm.NewBest) Center("NEW BEST!", _accent, -96);
                    Center("Score " + gm.Score + "    Distance " + Mathf.FloorToInt(gm.Distance) + " m", _mid, -56);
                    Center("Best " + gm.Best + "    Best distance " + Mathf.FloorToInt(gm.BestDistance) + " m", _small, -16);
                    Center("Coins +" + gm.Coins + "    Bank " + SaveSystem.Data.coins, _small, 12);
                    DrawCoreProgress(48);
                    Center("Swipe or Space to retry", _small, 100);
                    break;
            }
        }

        // Boss HP bar + weak-spot hint (GDD §7).
        void DrawBossBar()
        {
            var boss = BossSystem.Instance;
            if (boss == null || !boss.BossActive) return;

            const float w = 280f, h = 16f;
            float x = (Screen.width - w) / 2f;
            float y = 20f;

            // Label
            Center("!! LIEUTENANT !!", _mid, -Screen.height / 2f + 36f);

            // HP bar
            Color prev = GUI.color;
            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            GUI.color = new Color(1f, 0.3f, 0.2f);
            GUI.Box(new Rect(x + 2f, y + 2f, (w - 4f) * boss.BossHealth01, h - 4f), GUIContent.none);
            GUI.color = prev;

            // Weak-spot hint
            var ws = boss.CurrentWeakSpot;
            if (ws != SwipeDirection.None)
            {
                string hint = ws switch
                {
                    SwipeDirection.Up    => "SWIPE UP",
                    SwipeDirection.Down  => "SWIPE DOWN",
                    SwipeDirection.Left  => "SWIPE LEFT",
                    SwipeDirection.Right => "SWIPE RIGHT",
                    _                    => "TAP",
                };
                Center(">>> " + hint + " <<<", _accent, -Screen.height / 2f + 76f);
            }
        }

        // Progress map to the Core (GDD §2): best distance as the meta-goal readout.
        // On the results screen the run's own distance is overlaid as a brighter notch.
        void DrawCoreProgress(float yOffset)
        {
            var gm = GameManager.Instance;
            float best01 = Mathf.Clamp01(gm.BestDistance / Config.CoreDistance);

            const float w = 320f, h = 14f;
            float x = (Screen.width - w) / 2f;
            float y = Screen.height / 2f + yOffset;

            GUI.Box(new Rect(x, y, w, h), GUIContent.none);
            Color prev = GUI.color;
            GUI.color = new Color(0.55f, 0.92f, 1f);
            GUI.Box(new Rect(x + 2f, y + 2f, (w - 4f) * best01, h - 4f), GUIContent.none);

            if (gm.State == GameState.GameOver)
            {
                float run01 = Mathf.Clamp01(gm.Distance / Config.CoreDistance);
                GUI.color = new Color(1f, 0.9f, 0.3f);
                GUI.Box(new Rect(x + 2f + (w - 6f) * run01, y - 2f, 4f, h + 4f), GUIContent.none);
            }
            GUI.color = prev;

            Center("Path to the Core  " + Mathf.FloorToInt(best01 * 100f) + "%", _small, yOffset + 18f);
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
