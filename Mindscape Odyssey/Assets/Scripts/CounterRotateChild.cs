using UnityEngine;

public class CounterRotateChild : MonoBehaviour
{
    private Quaternion initialRotation; // To store the initial rotation of the child

    void Start()
    {
        // Save the initial local rotation of the child object
        initialRotation = transform.localRotation;
    }

    void LateUpdate()
    {
        // Counteract the parent's rotation by resetting to the initial rotation
        transform.rotation = initialRotation;
    }
}
