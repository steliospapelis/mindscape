using UnityEngine;
using System.Collections;

public class TilemapMover : MonoBehaviour
{
    public float moveDistance = 2f;  // Distance to move in each direction.
    public float speed = 1f;         // Speed of movement.
    public float pauseDuration = 0.5f; // Pause duration at each end.

    private Vector3 startPosition;
    private bool movingRight = true;
    private bool isPaused = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!isPaused)
        {
            float targetX = movingRight ? startPosition.x + moveDistance : startPosition.x - moveDistance;
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetX, startPosition.y, startPosition.z), speed * Time.deltaTime);

            // Check if we reached the target position
            if (Mathf.Approximately(transform.position.x, targetX))
            {
                StartCoroutine(PauseAndReverseDirection());
            }
        }
    }

    private IEnumerator PauseAndReverseDirection()
    {
        isPaused = true;
        yield return new WaitForSeconds(pauseDuration);

        // Switch direction after the pause
        movingRight = !movingRight;
        isPaused = false;
    }
}
