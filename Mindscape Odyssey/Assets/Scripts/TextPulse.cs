using UnityEngine;
using TMPro;

public class TextPulse : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float pulseSpeed = 1.2f;      // Controls how fast the pulse animation is
    public float minScale = 0.9f;      // Minimum scale of the pulse
    public float maxScale = 1.1f;      // Maximum scale of the pulse

    private bool isPulsingUp = true;   // Track if we are pulsing up or down
    private Vector3 originalScale;

    void Start()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        originalScale = text.transform.localScale; // Save the original scale
    }

    void Update()
    {
        if (isPulsingUp)
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, originalScale * maxScale, pulseSpeed * Time.deltaTime);
            if (text.transform.localScale.x >= originalScale.x * maxScale - 0.01f) // Close to max scale
            {
                isPulsingUp = false; // Start pulsing down
            }
        }
        else
        {
            text.transform.localScale = Vector3.Lerp(text.transform.localScale, originalScale * minScale, pulseSpeed * Time.deltaTime);
            if (text.transform.localScale.x <= originalScale.x * minScale + 0.01f) // Close to min scale
            {
                isPulsingUp = true; // Start pulsing up
            }
        }
    }
}
