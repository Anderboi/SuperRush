using UnityEngine;

namespace Ryvok
{
    /// <summary>
    /// Creates a simple unlit-ish colored material that works whether the project
    /// uses URP or the Built-in pipeline. Placeholder visuals only.
    /// </summary>
    public static class MaterialUtil
    {
        public static Material Colored(Color c)
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Sprites/Default");

            var m = new Material(s);
            // URP uses _BaseColor, Built-in Standard uses _Color.
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            m.color = c;
            return m;
        }

        public static readonly Color Up    = new Color(1f, 0.80f, 0.10f);   // yellow
        public static readonly Color Down  = new Color(1f, 0.30f, 0.36f);   // red
        public static readonly Color Side  = new Color(0.20f, 0.84f, 1f);   // blue
        public static readonly Color Tap   = new Color(0.70f, 0.40f, 1f);   // purple
        public static readonly Color Hero  = new Color(0.23f, 0.63f, 1f);
        public static readonly Color Ground= new Color(0.16f, 0.55f, 0.34f);
        public static readonly Color Stripe= new Color(0.12f, 0.42f, 0.26f);

        public static Color ForDirection(SwipeDirection d)
        {
            switch (d)
            {
                case SwipeDirection.Up:    return Up;
                case SwipeDirection.Down:  return Down;
                case SwipeDirection.Left:  return Side;
                case SwipeDirection.Right: return Side;
                default:                   return Tap;
            }
        }
    }
}
