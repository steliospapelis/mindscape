using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    public Transform target;  
    public float followSpeed = 0.2f;  
    public Vector3 offset;  

    private Vector3 shakeOffset = Vector3.zero;  
    private bool isShaking = false;  

    void FixedUpdate()
    {
        Vector3 targetPosition = target.position + offset + shakeOffset;  
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);  
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        if (!isShaking)
        {
            StartCoroutine(Shake(duration, magnitude));
        }
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;  
        isShaking = false;
    }
}
