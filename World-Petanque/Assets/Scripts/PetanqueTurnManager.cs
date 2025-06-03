using UnityEngine;
using System.Collections;
using Unity.MLAgents;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PetanqueTurnManager : MonoBehaviour
{
    public PetanqueAgent petanqueAgent;
    public Rigidbody agentBallRb;

    public Transform playerBallSpawn;
    public GameObject playerBallPrefab;

    public int maxThrows = 3;

    private int playerThrows = 0;
    private int agentThrows = 0;

    private GameObject currentPlayerBall;

    private float velocityThreshold = 0.05f;
    private float waitTimeAfterStop = 1f;

    private bool isPlayerTurn = true;
    private bool isWaitingForBallToStop = false;

    private bool agentHasThrown = false;

    void Start()
    {
        var requester = petanqueAgent.GetComponent<DecisionRequester>();
        if (requester != null)
        {
            requester.enabled = false;
            Debug.Log("[INIT] DecisionRequester disabled.");
        }

        petanqueAgent.enabled = false;
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        if (playerThrows >= maxThrows)
        {
            CheckGameOver();
            return;
        }

        Debug.Log($"[TURN] Player Turn {playerThrows + 1}");
        isPlayerTurn = true;
        isWaitingForBallToStop = false;

        if (currentPlayerBall != null)
            Destroy(currentPlayerBall);

        currentPlayerBall = Instantiate(playerBallPrefab, playerBallSpawn.position, Quaternion.identity);
        var grabInteractable = currentPlayerBall.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectExited.AddListener(OnPlayerGrabReleased);
        }
    }

    public void OnPlayerBallReleased()
    {
        if (!isPlayerTurn || isWaitingForBallToStop)
            return;

        Debug.Log("[ACTION] Player released the ball.");
        isWaitingForBallToStop = true;
        StartCoroutine(WaitForBallStop(currentPlayerBall.GetComponent<Rigidbody>(), OnPlayerBallStopped));
    }

    void OnPlayerBallStopped()
    {
        Debug.Log("[STATE] Player ball stopped.");
        playerThrows++;
        isPlayerTurn = false;
        isWaitingForBallToStop = false;

        StartCoroutine(DelayThenStartAgentTurn());
    }

    void OnPlayerGrabReleased(SelectExitEventArgs args)
    {
        Debug.Log("[INPUT] Player released the ball via XR controller.");
        OnPlayerBallReleased();
    }

    IEnumerator DelayThenStartAgentTurn()
    {
        yield return new WaitForSeconds(1f);
        StartAgentTurn();
    }

    IEnumerator MonitorAgentThrowAndStop()
    {
        Rigidbody rb = agentBallRb;
        float throwDetectThreshold = 0.5f;
        float timeout = 10f;
        float elapsed = 0f;

        while (!agentHasThrown && elapsed < timeout)
        {
            if (rb.velocity.magnitude > throwDetectThreshold)
            {
                Debug.Log("[MANAGER] Agent has thrown.");
                agentHasThrown = true;

                // Wait for ball to stop naturally
                StartCoroutine(WaitForBallStop(rb, OnAgentBallStopped));
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!agentHasThrown)
        {
            Debug.LogWarning("[MANAGER] Agent throw not detected. Forcing stop.");
            petanqueAgent.EndEpisode();
            petanqueAgent.enabled = false;
            OnAgentBallStopped();
        }
    }

    void StartAgentTurn()
    {
        if (agentThrows >= maxThrows)
        {
            CheckGameOver();
            return;
        }

        Debug.Log($"[TURN] Agent Turn {agentThrows + 1}");

        agentHasThrown = false;
        isWaitingForBallToStop = true;

        // Enable agent and manually request decision once
        petanqueAgent.enabled = true;

        var requester = petanqueAgent.GetComponent<DecisionRequester>();
        if (requester != null)
            requester.enabled = false;

        petanqueAgent.RequestDecision();

        // Start monitoring for throw and ball stop
        StartCoroutine(MonitorAgentThrowAndStop());
    }

    void OnAgentBallStopped()
    {
        Debug.Log("[STATE] Agent ball stopped.");
        agentThrows++;
        isWaitingForBallToStop = false;

        // Now safe to disable agent to prevent further throws
        petanqueAgent.enabled = false;

        if (playerThrows < maxThrows)
            StartCoroutine(DelayThenStartPlayerTurn());
        else
            CheckGameOver();
    }

    IEnumerator DelayThenStartPlayerTurn()
    {
        yield return new WaitForSeconds(1f);
        StartPlayerTurn();
    }

    IEnumerator WaitForBallStop(Rigidbody rb, System.Action onStop)
    {
        while (true)
        {
            if (rb.velocity.magnitude < velocityThreshold && rb.angularVelocity.magnitude < velocityThreshold)
            {
                yield return new WaitForSeconds(waitTimeAfterStop);

                if (rb.velocity.magnitude < velocityThreshold && rb.angularVelocity.magnitude < velocityThreshold)
                {
                    onStop?.Invoke();
                    yield break;
                }
            }
            yield return null;
        }
    }

    void CheckGameOver()
    {
        if (playerThrows >= maxThrows && agentThrows >= maxThrows)
        {
            Debug.Log("[GAME] Game Over. All throws completed.");
            // Add scoring or end game logic here
        }
    }
}
