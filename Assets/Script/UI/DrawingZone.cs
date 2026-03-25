using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Zone de dessin tactile basée sur une RenderTexture.
/// Le joueur dessine en glissant le doigt dans la zone du RawImage.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class DrawingZone : MonoBehaviour
{
    private const string DrawingShaderColor = "_Color";

    [Header("Dessin")]
    [SerializeField] private Color drawColor    = Color.black;
    [SerializeField] private int   brushSize    = 10;

    [Header("Texture")]
    [SerializeField] private int textureWidth  = 512;
    [SerializeField] private int textureHeight = 512;
    [SerializeField] private Color backgroundColor = Color.white;

    [Header("Références")]
    [SerializeField] private Camera uiCamera;

    private RawImage   _rawImage;
    private Texture2D  _drawTexture;
    private RectTransform _rectTransform;

    private Vector2 _previousTouchPosition = Vector2.negativeInfinity;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        EnhancedTouchSupport.Enable();

        _rawImage      = GetComponent<RawImage>();
        _rectTransform = GetComponent<RectTransform>();

        InitTexture();
    }

    private void OnDestroy()
    {
        if (_drawTexture != null)
            Destroy(_drawTexture);
    }

    private void Update()
    {
        if (Touch.activeTouches.Count == 0)
        {
            _previousTouchPosition = Vector2.negativeInfinity;
            return;
        }

        UnityEngine.InputSystem.EnhancedTouch.Touch touch = Touch.activeTouches[0];

        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
            touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
        {
            _previousTouchPosition = Vector2.negativeInfinity;
            return;
        }

        Vector2 localPoint;
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rectTransform,
            touch.screenPosition,
            uiCamera,
            out localPoint
        );

        if (!isInside) return;

        Vector2 texturePoint = LocalToTexturePoint(localPoint);

        if (_previousTouchPosition != Vector2.negativeInfinity)
            DrawLine(_previousTouchPosition, texturePoint);
        else
            DrawCircle(Mathf.RoundToInt(texturePoint.x), Mathf.RoundToInt(texturePoint.y));

        _previousTouchPosition = texturePoint;
        _drawTexture.Apply();
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>Efface entièrement la zone de dessin.</summary>
    public void ClearDrawing()
    {
        Color32[] pixels = new Color32[textureWidth * textureHeight];
        Color32   bg32   = backgroundColor;

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = bg32;

        _drawTexture.SetPixels32(pixels);
        _drawTexture.Apply();
    }

    /// <summary>Change la couleur du pinceau.</summary>
    public void SetDrawColor(Color color)
    {
        drawColor = color;
    }

    /// <summary>Change la taille du pinceau.</summary>
    public void SetBrushSize(int size)
    {
        brushSize = Mathf.Max(1, size);
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    private void InitTexture()
    {
        _drawTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode   = TextureWrapMode.Clamp
        };

        ClearDrawing();
        _rawImage.texture = _drawTexture;
    }

    // ── Dessin ────────────────────────────────────────────────────────────────

    private void DrawLine(Vector2 from, Vector2 to)
    {
        float distance  = Vector2.Distance(from, to);
        int   steps     = Mathf.Max(1, Mathf.CeilToInt(distance));
        float stepSize  = 1f / steps;

        for (int i = 0; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(from, to, i * stepSize);
            DrawCircle(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
        }
    }

    private void DrawCircle(int centerX, int centerY)
    {
        int radius = brushSize / 2;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y > radius * radius) continue;

                int px = centerX + x;
                int py = centerY + y;

                if (px < 0 || px >= textureWidth || py < 0 || py >= textureHeight) continue;

                _drawTexture.SetPixel(px, py, drawColor);
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Vector2 LocalToTexturePoint(Vector2 localPoint)
    {
        Rect rect = _rectTransform.rect;

        float normalizedX = (localPoint.x - rect.x) / rect.width;
        float normalizedY = (localPoint.y - rect.y) / rect.height;

        return new Vector2(
            normalizedX * textureWidth,
            normalizedY * textureHeight
        );
    }
}
