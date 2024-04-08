using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target; 
    public float followSpeed = 0.2f; 
    public Vector3 offset; 


    void FixedUpdate()
    {
        
        Vector3 targetPosition = target.position + offset;

        
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
