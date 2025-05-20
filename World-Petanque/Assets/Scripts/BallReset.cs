using UnityEngine;

public class BallReset : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    public float resetDelay = 3f; // tijd voordat reset gebeurt
    public float stopThreshold = 0.05f; // hoe langzaam de bal moet zijn om als "stil" te tellen

    private bool isResetting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        // Check of de bal stil ligt
        if (!isResetting && rb.linearVelocity.magnitude < stopThreshold && rb.angularVelocity.magnitude < stopThreshold)
        {
            isResetting = true;
            Invoke("ResetBall", resetDelay);
        }
    }

    void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
        transform.rotation = startRotation;
        isResetting = false;
    }
}
