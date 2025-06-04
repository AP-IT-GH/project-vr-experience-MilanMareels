using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallResetZone : MonoBehaviour
{
    [Header("Assign Both Balls")]
    public GameObject playerBall;
    public GameObject agentBall;

    public int resetTime;

    private Dictionary<GameObject, BallData> ballDataMap = new Dictionary<GameObject, BallData>();

    void Start()
    {
        SetupBall(playerBall);
        SetupBall(agentBall);
    }

    void SetupBall(GameObject ball)
    {
        if (ball != null)
        {
            BallData data = new BallData
            {
                originalPosition = ball.transform.position,
                originalRotation = ball.transform.rotation,
                rb = ball.GetComponent<Rigidbody>()
            };

            ballDataMap[ball] = data;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ballDataMap.ContainsKey(other.gameObject))
        {
            BallData data = ballDataMap[other.gameObject];

            if (data.resetCoroutine == null)
                data.resetCoroutine = StartCoroutine(WaitAndReset(other.gameObject));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ballDataMap.ContainsKey(other.gameObject))
        {
            BallData data = ballDataMap[other.gameObject];

            if (data.resetCoroutine != null)
            {
                StopCoroutine(data.resetCoroutine);
                data.resetCoroutine = null;
            }
        }
    }

    IEnumerator WaitAndReset(GameObject ball)
    {
        yield return new WaitForSeconds(resetTime);

        BallData data = ballDataMap[ball];

        ball.transform.position = data.originalPosition;
        ball.transform.rotation = data.originalRotation;

        if (data.rb != null)
        {
            data.rb.linearVelocity = Vector3.zero;
            data.rb.angularVelocity = Vector3.zero;
        }

        data.resetCoroutine = null;
    }

    class BallData
    {
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Rigidbody rb;
        public Coroutine resetCoroutine;
    }
}
