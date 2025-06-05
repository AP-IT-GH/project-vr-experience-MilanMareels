using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject playerBall;
    public GameObject agentBall;
    public GameObject agent;
    public Transform playerBallStart;
    public Transform agentBallStart;

    private enum GameState { PlayerTurn, AgentTurn, Waiting }
    private GameState currentState = GameState.PlayerTurn;

    private bool playerBallStopped = false;
    private bool agentBallStopped = false;

    private GameState lastLoggedState = GameState.Waiting;

    private float playerBallStillTime = 0f;
    private float waitToCheckStill = 0.2f;
    private bool hasAgentThrown = false;


    void Update()
    {
        if (currentState != lastLoggedState)
        {
            Debug.Log("Nieuwe staat: " + currentState);
            lastLoggedState = currentState;
        }

        switch (currentState)
        {
            case GameState.PlayerTurn:
                if (playerBall.GetComponent<PlayerBall>().wasThrown)
                {
                    if (playerBall.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.05f)
                    {
                        playerBallStillTime += Time.deltaTime;

                        if (playerBallStillTime >= waitToCheckStill)
                        {
                            currentState = GameState.AgentTurn;
                            Invoke(nameof(StartAgentThrow), 1f);
                        }
                    }
                    else
                    {
                        playerBallStillTime = 0f;
                    }
                }
                break;

            case GameState.AgentTurn:
                if (hasAgentThrown && agentBall.GetComponent<Rigidbody>().linearVelocity.magnitude < 0.05f)
                {
                    currentState = GameState.Waiting;
                    Invoke(nameof(CalculateScore), 1f);
                }
                break;
        }
    }


    void StartAgentThrow()
    {
        Debug.Log("Agent beurt start");
        PetanqueAgent petanqueAgent = agent.GetComponent<PetanqueAgent>();
        petanqueAgent.EndEpisode();
        Invoke(nameof(EnableAgentThrow), 0.1f);
    }

    void EnableAgentThrow()
    {
        PetanqueAgent petanqueAgent = agent.GetComponent<PetanqueAgent>();
        petanqueAgent.BeginAgentThrow();
        hasAgentThrown = true;
    }


    void CalculateScore()
    {
        float distPlayer = Vector3.Distance(playerBall.transform.position, agent.GetComponent<PetanqueAgent>().target.position);
        float distAgent = Vector3.Distance(agentBall.transform.position, agent.GetComponent<PetanqueAgent>().target.position);

        if (distPlayer < distAgent)
            Scoreboard.Instance.AddPointToPlayer();
        else
            Scoreboard.Instance.AddPointToAgent();

        ResetRound();
    }

    void ResetRound()
    {
        // Reset ballen
        playerBall.transform.position = playerBallStart.position;
        playerBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        playerBall.GetComponent<PlayerBall>().wasThrown = false;
        playerBall.GetComponent<PlayerBall>().ResetBall();

        agentBall.transform.position = agentBallStart.position;
        agentBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

        currentState = GameState.PlayerTurn;
        playerBallStopped = false;
        agentBallStopped = false;
    }
}
