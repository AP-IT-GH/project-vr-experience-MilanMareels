using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PetanqueAgent : Agent
{
    [Header("References")]
    public Rigidbody ballRb;
    public Transform target;
    public Transform ballStartPos;

    [Header("Settings")]
    public float groundLevel = 0f;
    public float maxThrowPower = 30f;
    public float maxDistance = 10f;

    private bool hasThrown = false;
    private Vector3 initialTargetPosition;

    public bool allowThrow = false;


    public override void Initialize()
    {
        initialTargetPosition = target.position;
        ballRb.maxAngularVelocity = 20f;
    }


    public override void OnEpisodeBegin()
    {
        Debug.Log("Agent: OnEpisodeBegin");
        Debug.Log("Episode begint. Pos bal: " + ballRb.position + ", Target: " + target.position);

        hasThrown = false;
        allowThrow = false;
        // Reset bal
        ballRb.transform.position = ballStartPos.position;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Reset target (optioneel randomiseren)
        // target.position = initialTargetPosition + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));
    }
    public void BeginAgentThrow()
    {
        Debug.Log("Agent: BeginAgentThrow -> mag gooien");
        allowThrow = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Relatieve positie tot target (3)
        sensor.AddObservation(target.position - ballRb.position);

        // 2. Snelheid van de bal (3)
        sensor.AddObservation(ballRb.linearVelocity);

        // 3. Positie van de bal (3)
        sensor.AddObservation(ballRb.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        

        if (!allowThrow)
        {
            return;
        }

        if (hasThrown)
        {
            return;
        }

        Debug.Log("Actie ontvangen. allowThrow=" + allowThrow + " | hasThrown=" + hasThrown);
        float dirX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float dirY = Mathf.Clamp(actions.ContinuousActions[1], 0.1f, 1f);
        float dirZ = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
        float power = Mathf.Clamp(actions.ContinuousActions[3], 0.1f, 1f) * maxThrowPower;

        Vector3 direction = new Vector3(dirX, dirY, dirZ).normalized;
        ballRb.AddForce(direction * power, ForceMode.VelocityChange);

        hasThrown = true;
        

        // End episode als bal van terrein valt
        if (ballRb.position.y < groundLevel - 0.5f)
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        // Controleer of bal stilstaat
        if (hasThrown && ballRb.linearVelocity.magnitude <= 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float normalizedDist = Mathf.Clamp01(dist / maxDistance);

            float reward = 1f - normalizedDist; // 1 als perfect, 0 als slecht

            if (dist < 0.2f)
            {
                reward += 1f; // grote bonus als zeer dichtbij
            }
            else if (dist > maxDistance * 0.9f)
            {
                reward -= 0.5f; // straf bij grote mis
            }

            AddReward(reward);
            EndEpisode();
            return;
        }

        // Tussentijdse shaping reward als bal beweegt
        if (hasThrown)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float shapedReward = Mathf.Clamp01(1f - (dist / maxDistance));
            AddReward(shapedReward * 0.005f); // versterkt shaping
        }

        // Straf voor tijd
        AddReward(-0.001f); // licht verhoogde straf per stap
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal"); // x richting
        actions[1] = 0.5f;                        // y richting
        actions[2] = Input.GetAxis("Vertical");   // z richting
        actions[3] = Input.GetKey(KeyCode.Space) ? 1f : 0.7f; // kracht
    }
}
