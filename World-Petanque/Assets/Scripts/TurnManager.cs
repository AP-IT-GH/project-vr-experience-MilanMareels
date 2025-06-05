using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public GameObject playerBall;
    public GameObject agentBall;
    public GameObject agent;
    public Transform playerBallReset;
    public Transform agentBallReset;
    public Transform agentBallThrowStart;

    private enum GameState { PlayerTurn, AgentTurn, Waiting }
    private GameState currentState = GameState.PlayerTurn;

    private bool playerBallStopped = false;
    private bool agentBallStopped = false;

    private GameState lastLoggedState = GameState.Waiting;

    private float playerBallStillTime = 0f;
    private float waitToCheckStill = 0.2f;
    private bool hasAgentThrown = false;

    public int playerScore = 0;
    public int agentScore = 0;
    public int maxScore = 3;

    public GameObject fireworksPrefab;
    public TextMeshProUGUI winText;
    public GameObject winCanvas;


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
                    Invoke(nameof(CalculateScore), 4f);
                }
                break;
        }
    }


    void StartAgentThrow()
    {
        Debug.Log("Agent beurt start");
        Animator animator = agent.GetComponentInChildren<Animator>();
        animator.SetTrigger("Throw");
        agentBall.transform.position = agentBallThrowStart.position;
        agentBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        agentBall.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        PetanqueAgent petanqueAgent = agent.GetComponent<PetanqueAgent>();
        petanqueAgent.EndEpisode();
        Invoke(nameof(EnableAgentThrow), 1.8f);
    }

    void EnableAgentThrow()
    {
        Debug.Log("Agent mag nu gooien");
        PetanqueAgent petanqueAgent = agent.GetComponent<PetanqueAgent>();
        petanqueAgent.BeginAgentThrow();
        hasAgentThrown = true;
    }


    void CalculateScore()
    {
        float distPlayer = Vector3.Distance(playerBall.transform.position, agent.GetComponent<PetanqueAgent>().target.position);
        float distAgent = Vector3.Distance(agentBall.transform.position, agent.GetComponent<PetanqueAgent>().target.position);

        if (distPlayer < distAgent)
        {
            Scoreboard.Instance.AddPointToPlayer();
            AddPointToPlayer();
        }
        else
        {
            Scoreboard.Instance.AddPointToAgent();
            AddPointToAgent();
        }

        if (playerScore < maxScore && agentScore < maxScore)
        {
            ResetRound();
        }
    }

    public void AddPointToPlayer()
    {
        playerScore++;
        if (playerScore >= maxScore)
            EndGame("Player wint!");
    }

    public void AddPointToAgent()
    {
        agentScore++;
        if (agentScore >= maxScore)
            EndGame("Agent wint!");
    }

    void EndGame(string winnerText)
    {
        Debug.Log("Einde spel: " + winnerText);

        if (fireworksPrefab != null)
        {
            fireworksPrefab.SetActive(true);
        }

        if (winCanvas != null) winCanvas.SetActive(true);
        if (winText != null) winText.text = winnerText;
    }

    void ResetRound()
    {
        // Reset ballen
        playerBall.transform.position = playerBallReset.position;
        playerBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        playerBall.GetComponent<PlayerBall>().wasThrown = false;
        playerBall.GetComponent<PlayerBall>().ResetBall();

        agentBall.transform.position = agentBallReset.position;
        agentBall.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;

        playerBallStillTime = 0f;

        currentState = GameState.PlayerTurn;
        playerBallStopped = false;
        agentBallStopped = false;
        hasAgentThrown = false;
    }
}
