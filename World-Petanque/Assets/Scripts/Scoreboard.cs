using UnityEngine;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard Instance;

    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI agentScoreText;

    private int playerScore = 0;
    private int agentScore = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddPointToPlayer()
    {
        playerScore++;
        UpdateUI();
    }

    public void AddPointToAgent()
    {
        agentScore++;
        UpdateUI();
    }

    void UpdateUI()
    {
        playerScoreText.text = "" + playerScore;
        agentScoreText.text = "" + agentScore;
    }
}