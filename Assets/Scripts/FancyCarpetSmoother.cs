using UnityEngine;
using UnityEngine.UI;

public class FancyCarpetSmoother : MonoBehaviour
{
    public Texture2D regularCarpet;
    public Texture2D smoothedCarpet;
    public Material carpetMaterial; // Assign the material using CarpetBlend.shader
    public RawImage carpetDisplay;
    public int brushRadius = 64;
    public Texture2D maskTexture;

    void Start()
    {
        // Create black mask
        maskTexture = new Texture2D(regularCarpet.width, regularCarpet.height, TextureFormat.RGBA32, false);
        maskTexture.filterMode = FilterMode.Bilinear;
        ClearMask();

        // Assign material and textures
        carpetMaterial.SetTexture("_MainTex", regularCarpet);
        carpetMaterial.SetTexture("_SmoothTex", smoothedCarpet);
        carpetMaterial.SetTexture("_MaskTex", maskTexture);

        carpetDisplay.material = carpetMaterial;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 uv;
            if (ScreenPointToUV(mousePos, out uv))
            {
                DrawSoftCircleOnMask(uv);
            }
        }
    }

    void ClearMask()
    {
        Color clear = new Color(0, 0, 0, 1);
        for (int y = 0; y < maskTexture.height; y++)
        {
            for (int x = 0; x < maskTexture.width; x++)
            {
                maskTexture.SetPixel(x, y, clear);
            }
        }
        maskTexture.Apply();
    }

    bool ScreenPointToUV(Vector2 screenPos, out Vector2 uv)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            carpetDisplay.rectTransform, screenPos, null, out localPoint
        );

        Rect rect = carpetDisplay.rectTransform.rect;
        uv = new Vector2(
            Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x),
            Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y)
        );

        return uv.x >= 0f && uv.x <= 1f && uv.y >= 0f && uv.y <= 1f;
    }

    void DrawSoftCircleOnMask(Vector2 uv)
    {
        int centerX = (int)(uv.x * maskTexture.width);
        int centerY = (int)(uv.y * maskTexture.height);

        for (int y = -brushRadius; y <= brushRadius; y++)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;
                if (px >= 0 && px < maskTexture.width && py >= 0 && py < maskTexture.height)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist <= brushRadius)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist / brushRadius)); // 1 at center, 0 at edge
                        Color existing = maskTexture.GetPixel(px, py);
                        float newAlpha = Mathf.Clamp01(existing.r + alpha * 0.5f); // slowly build up
                        maskTexture.SetPixel(px, py, new Color(newAlpha, newAlpha, newAlpha, 1f));
                    }
                }
            }
        }
        maskTexture.Apply();
    }
}
