using UnityEngine;
using UnityEngine.InputSystem;

public class HandCursor : MonoBehaviour
{
    public Texture2D cursorTextureHandOpen;
    public Texture2D cursorTextureHandClosed;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        Cursor.SetCursor(cursorTextureHandOpen, hotSpot, cursorMode);
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Cursor.SetCursor(cursorTextureHandClosed, hotSpot, cursorMode);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Cursor.SetCursor(cursorTextureHandOpen, hotSpot, cursorMode);
        }
    }
}
