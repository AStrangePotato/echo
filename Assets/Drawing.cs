using UnityEngine;
using UnityEngine.UI;

public class DrawingCanvas : MonoBehaviour
{
    private RawImage rawImage;
    private Texture2D texture;
    private RectTransform rectTransform;

    private Vector2? lastMousePos = null; // Track last mouse position
    public Color drawColor = Color.black;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        // Create a new transparent Texture2D
        texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);

        // Fill texture with transparent pixels
        Color[] pixels = new Color[texture.width * texture.height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color(0, 0, 0, 0); // fully transparent
        texture.SetPixels(pixels);
        texture.Apply();

        rawImage.texture = texture;
        rawImage.color = Color.white; // ensure overlay is visible
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 localPos;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPos))
                return;

            Vector2 normalizedPos = new Vector2(
                (localPos.x / rectTransform.rect.width) + 0.5f,
                (localPos.y / rectTransform.rect.height) + 0.5f
            );

            int x = Mathf.FloorToInt(normalizedPos.x * texture.width);
            int y = Mathf.FloorToInt(normalizedPos.y * texture.height);

            if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
            {
                if (lastMousePos.HasValue)
                {
                    DrawLine((int)lastMousePos.Value.x, (int)lastMousePos.Value.y, x, y, drawColor);
                }
                else
                {
                    texture.SetPixel(x, y, drawColor);
                }

                texture.Apply();
                lastMousePos = new Vector2(x, y);
            }
        }
        else
        {
            lastMousePos = null; // Reset when not drawing
        }
    }

    // Bresenham's line algorithm for connected pixels
    private void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (x0 >= 0 && x0 < texture.width && y0 >= 0 && y0 < texture.height)
                texture.SetPixel(x0, y0, color);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }
        }
    }
}
