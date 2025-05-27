using System.Collections;
using UnityEngine;

public class BallResetZone : MonoBehaviour
{
    public GameObject ball;

    public int resetTime;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody ballRb;
    private Coroutine resetCoroutine;

    void Start()
    {
        if (ball != null)
        {
            originalPosition = ball.transform.position;
            originalRotation = ball.transform.rotation;
            ballRb = ball.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ball)
        {
            // Start waiting to reset
            if (resetCoroutine == null)
                resetCoroutine = StartCoroutine(WaitAndReset());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == ball)
        {
            // Cancel the reset if the ball leaves early
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
                resetCoroutine = null;
            }
        }
    }

    IEnumerator WaitAndReset()
    {
        yield return new WaitForSeconds(resetTime);

        // Reset position, rotation and velocity
        ball.transform.position = originalPosition;
        ball.transform.rotation = originalRotation;

        if (ballRb != null)
        {
            ballRb.linearVelocity = Vector3.zero;
            ballRb.angularVelocity = Vector3.zero;
        }

        resetCoroutine = null;
    }
}
