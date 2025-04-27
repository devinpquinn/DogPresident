using UnityEngine;
using UnityEngine.UI;

public class FancyCarpetSmoother : MonoBehaviour
{
    [Header("Carpet Textures")]
    public Texture2D regularCarpet;
    public Texture2D smoothedCarpet;

    [Header("Display Settings")]
    public RawImage carpetDisplay;
    public Material carpetMaterial;

    [Header("Brush Settings")]
    public int brushRadius = 20;

    private Texture2D maskTexture;
    private Color[] maskPixels;
    private int texWidth;
    private int texHeight;

    void Start()
    {
        texWidth = regularCarpet.width;
        texHeight = regularCarpet.height;

        maskTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        maskTexture.filterMode = FilterMode.Bilinear;

        maskPixels = new Color[texWidth * texHeight];
        ClearMask();

        carpetMaterial.SetTexture("_MainTex", regularCarpet);
        carpetMaterial.SetTexture("_SmoothTex", smoothedCarpet);
        carpetMaterial.SetTexture("_MaskTex", maskTexture);

        carpetDisplay.material = carpetMaterial;
    }

    void Update()
    {
        // only on initial mouse click, not while holding
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                carpetDisplay.rectTransform,
                mousePos,
                null,
                out Vector2 localPoint
            );

            Vector2 uv = new Vector2(
                (localPoint.x + carpetDisplay.rectTransform.rect.width * 0.5f) / carpetDisplay.rectTransform.rect.width,
                (localPoint.y + carpetDisplay.rectTransform.rect.height * 0.5f) / carpetDisplay.rectTransform.rect.height
            );

            DrawSoftCircleOnMask(uv);
        }
    }

    void DrawSoftCircleOnMask(Vector2 uv)
    {
        int centerX = (int)(uv.x * texWidth);
        int centerY = (int)(uv.y * texHeight);

        for (int y = -brushRadius; y <= brushRadius; y++)
        {
            for (int x = -brushRadius; x <= brushRadius; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px >= 0 && px < texWidth && py >= 0 && py < texHeight)
                {
                    float dist = Mathf.Sqrt(x * x + y * y);
                    if (dist <= brushRadius)
                    {
                        float brushAlpha = Mathf.Pow(Mathf.Clamp01(1f - (dist / brushRadius)), 2f); // soft falloff

                        int idx = py * texWidth + px;
                        float finalAlpha = Mathf.Max(maskPixels[idx].r, brushAlpha);
                        maskPixels[idx] = new Color(finalAlpha, finalAlpha, finalAlpha, 1f);
                    }
                }
            }
        }

        maskTexture.SetPixels(maskPixels);
        maskTexture.Apply();
    }

    void ClearMask()
    {
        for (int i = 0; i < maskPixels.Length; i++)
        {
            maskPixels[i] = new Color(0f, 0f, 0f, 1f);
        }

        maskTexture.SetPixels(maskPixels);
        maskTexture.Apply();
    }
}
