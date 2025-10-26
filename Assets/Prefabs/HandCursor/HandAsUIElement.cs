using UnityEngine;
using UnityEngine.InputSystem;

public class HandAsUIElement : MonoBehaviour
{
    public RectTransform openHandTransform;
    public RectTransform closedHandTransform;
    public Camera cam;

    public GameObject openHand;
    public GameObject closedHand;

    void Start()
    {
        closedHand.SetActive(false);
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();  // new input system

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform as RectTransform,
            mousePos,
            cam,
            out var localPos
        );

        openHandTransform.anchoredPosition = localPos;
        closedHandTransform.anchoredPosition = localPos;
        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            openHand.SetActive(false);
            closedHand.SetActive(true);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            openHand.SetActive(true);
            closedHand.SetActive(false);
        }
    }

    void OnEnable()
    {
        Cursor.visible = false;
    }

    void OnDisable()
    {
        Cursor.visible = true;
    }
}
