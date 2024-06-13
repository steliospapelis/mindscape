using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PointDown: MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Button yourButton; // Reference to the Button component
    public Vector3 pressedScale = new Vector3(0.27f, 0.45f, 1f); // Scale when button is pressed
    public Vector3 normalScale = new Vector3(0.3f, 0.5f, 1f); // Normal scale of the button

    void Start()
    {
        // Set the initial scale of the button
        yourButton.transform.localScale = normalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Change the scale of the button when pressed
        yourButton.transform.localScale = pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Reset the scale of the button when released
        yourButton.transform.localScale = normalScale;
    }
}

