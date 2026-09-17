using UnityEngine;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public static class SurexsVisualTheme
    {
        public static Color Background => new Color(0.025f, 0.035f, 0.075f, 1f);
        public static Color BackgroundGlow => new Color(0.05f, 0.16f, 0.28f, 1f);
        public static Color Surface => new Color(0.055f, 0.075f, 0.14f, 0.96f);
        public static Color SurfaceRaised => new Color(0.08f, 0.115f, 0.20f, 0.98f);
        public static Color Primary => new Color(0.08f, 0.55f, 0.92f, 1f);
        public static Color Secondary => new Color(0.75f, 0.23f, 0.88f, 1f);
        public static Color Accent => new Color(1f, 0.76f, 0.16f, 1f);
        public static Color Success => new Color(0.20f, 0.92f, 0.58f, 1f);
        public static Color Error => new Color(1f, 0.25f, 0.38f, 1f);
        public static Color TextPrimary => new Color(0.96f, 0.98f, 1f, 1f);
        public static Color TextSecondary => new Color(0.60f, 0.72f, 0.88f, 1f);

        private static Sprite roundedSprite;

        public static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static void ApplyRounded(Image image)
        {
            image.sprite = RoundedSprite;
            image.type = Image.Type.Sliced;
        }

        public static void StyleButton(Button button, Color normal)
        {
            var image = button.targetGraphic as Image;
            if (image != null) ApplyRounded(image);
            var colors = ColorBlock.defaultColorBlock;
            colors.normalColor = normal;
            colors.highlightedColor = Color.Lerp(normal, Color.white, 0.22f);
            colors.selectedColor = Color.Lerp(normal, Accent, 0.28f);
            colors.pressedColor = Color.Lerp(normal, Color.black, 0.28f);
            colors.disabledColor = new Color(0.16f, 0.18f, 0.24f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (button.GetComponent<ArcadeButtonMotion>() == null) button.gameObject.AddComponent<ArcadeButtonMotion>();
        }

        private static Sprite RoundedSprite
        {
            get
            {
                if (roundedSprite != null) return roundedSprite;
                const int size = 64;
                const int radius = 14;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Runtime Rounded UI" };
                texture.wrapMode = TextureWrapMode.Clamp;
                var pixels = new Color32[size * size];
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = Mathf.Max(radius - x, 0, x - (size - radius - 1));
                    var dy = Mathf.Max(radius - y, 0, y - (size - radius - 1));
                    pixels[y * size + x] = dx * dx + dy * dy <= radius * radius
                        ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
                texture.SetPixels32(pixels); texture.Apply();
                roundedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), 100f, 0,
                    SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
                roundedSprite.name = "Runtime Rounded UI Sprite";
                return roundedSprite;
            }
        }
    }
}
