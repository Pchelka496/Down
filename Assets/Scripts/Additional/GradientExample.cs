using UnityEngine;
using UnityEngine.UI;

public static class GradientExample
{
    public enum GradientDirection
    {
        Horizontal,
        Vertical,
        DiagonalTopLeftToBottomRight,
        DiagonalBottomLeftToTopRight
    }

    public static Texture2D GenerateGradientTexture(Gradient gradient, int width, int height, GradientDirection direction)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float t = CalculateGradientPosition(x, y, width, height, direction);
                Color color = gradient.Evaluate(t);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        return texture;
    }

    private static float CalculateGradientPosition(int x, int y, int width, int height, GradientDirection direction)
    {
        return direction switch
        {
            GradientDirection.Horizontal => x / (float)(width - 1),
            GradientDirection.Vertical => y / (float)(height - 1),
            GradientDirection.DiagonalTopLeftToBottomRight => (x + y) / (float)(width + height - 2),
            GradientDirection.DiagonalBottomLeftToTopRight => (x + (height - 1 - y)) / (float)(width + height - 2),
            _ => 0f,
        };
    }

    public static void ApplyTextureToImage(Image image, Texture2D texture)
    {
        Rect spriteRect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);

        var sprite = Sprite.Create(texture, spriteRect, pivot);
        image.sprite = sprite;

        image.type = Image.Type.Simple;
        image.preserveAspect = true;
    }

}


