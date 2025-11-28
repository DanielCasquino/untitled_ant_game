using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    float cursorWidthScreenProportion = 0.5f; // cursor circle diameter is screen width (radius is 50%)
    public Vector2 mousePosition { get; private set; }

    public void SetMousePosition(Vector2 value)
    {
        mousePosition = value;
    }

    void Update()
    {
        Vector2 mousePos = mousePosition;
        Vector2 halfScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);

        float relativeRadius = Screen.width * cursorWidthScreenProportion;
        Vector2 screenSpaceCursor = (mousePos - halfScreen).normalized * relativeRadius + halfScreen;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenSpaceCursor.x, screenSpaceCursor.y, Camera.main.nearClipPlane));
        transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}