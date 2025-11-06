using UnityEngine;

public class SelectionBoxUI : MonoBehaviour
{
    [SerializeField]
    private Color boxColor = Color.green;

    [SerializeField]
    private float lineWidth = 2f;

    private Vector2 startPos;
    private Vector2 currentPos;
    private bool isSelecting = false;

    private Texture2D whiteTexture;

    private void Awake()
    {
        // Create a white texture for drawing lines
        whiteTexture = new Texture2D(1, 1);
        whiteTexture.SetPixel(0, 0, Color.white);
        whiteTexture.Apply();
    }

    public void StartSelection(Vector2 screenStart)
    {
        startPos = screenStart;
        currentPos = screenStart;
        isSelecting = true;
    }

    public void UpdateSelection(Vector2 screenCurrent)
    {
        currentPos = screenCurrent;
    }

    public void EndSelection()
    {
        isSelecting = false;
    }

    private void OnGUI()
    {
        if (!isSelecting)
            return;

        // Convert Unity screen coordinates (bottom-left origin) to GUI coordinates (top-left origin)
        Vector2 guiStart = new Vector2(startPos.x, Screen.height - startPos.y);
        Vector2 guiEnd = new Vector2(currentPos.x, Screen.height - currentPos.y);

        // Calculate rectangle bounds
        float left = Mathf.Min(guiStart.x, guiEnd.x);
        float right = Mathf.Max(guiStart.x, guiEnd.x);
        float top = Mathf.Min(guiStart.y, guiEnd.y);
        float bottom = Mathf.Max(guiStart.y, guiEnd.y);

        Rect rect = new Rect(left, top, right - left, bottom - top);

        // Draw the selection box
        DrawRect(rect, boxColor);
    }

    private void DrawRect(Rect rect, Color color)
    {
        GUI.color = color;

        // Top line
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, lineWidth), whiteTexture);
        // Bottom line
        GUI.DrawTexture(
            new Rect(rect.x, rect.y + rect.height - lineWidth, rect.width, lineWidth),
            whiteTexture
        );
        // Left line
        GUI.DrawTexture(new Rect(rect.x, rect.y, lineWidth, rect.height), whiteTexture);
        // Right line
        GUI.DrawTexture(
            new Rect(rect.x + rect.width - lineWidth, rect.y, lineWidth, rect.height),
            whiteTexture
        );

        GUI.color = Color.white;
    }

    public Rect GetScreenRect()
    {
        Vector2 boxStart = new Vector2(
            Mathf.Min(startPos.x, currentPos.x),
            Mathf.Min(startPos.y, currentPos.y)
        );

        Vector2 boxSize = new Vector2(
            Mathf.Abs(startPos.x - currentPos.x),
            Mathf.Abs(startPos.y - currentPos.y)
        );

        return new Rect(boxStart, boxSize);
    }

    private void OnDestroy()
    {
        if (whiteTexture != null)
        {
            Destroy(whiteTexture);
        }
    }
}
