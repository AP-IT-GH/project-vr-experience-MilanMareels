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
    public float maxThrowPower = 15f;
    public float maxDistance = 10f;

    private bool hasThrown = false;
    private Vector3 initialTargetPosition;

    public override void Initialize()
    {
        initialTargetPosition = target.position;
        ballRb.maxAngularVelocity = 20f;
    }

    public override void OnEpisodeBegin()
    {
        // Reset bal
        ballRb.transform.position = ballStartPos.position;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Reset target (optioneel randomiseren)
        // target.position = initialTargetPosition + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));

        hasThrown = false;
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
        if (!hasThrown)
        {
            // Acties: richting x,y,z en power
            float dirX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float dirY = Mathf.Clamp(actions.ContinuousActions[1], 0.1f, 1f);
            float dirZ = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
            float power = Mathf.Clamp(actions.ContinuousActions[3], 0.1f, 1f) * maxThrowPower;

            Vector3 direction = new Vector3(dirX, dirY, dirZ).normalized;
            ballRb.AddForce(direction * power, ForceMode.VelocityChange);

            hasThrown = true;
        }

        // Straf als de bal valt
        if (ballRb.position.y < groundLevel - 0.5f)
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        // Shaping reward tijdens beweging
        if (hasThrown && ballRb.linearVelocity.magnitude > 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float reward = Mathf.Clamp01(1f - (dist / maxDistance));
            AddReward(reward * 0.01f);
        }

        // Einde als bal stilstaat
        if (hasThrown && ballRb.linearVelocity.magnitude <= 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float normalizedDist = Mathf.Clamp01(dist / maxDistance);
            float finalReward = 1f - normalizedDist;

            // Straf bij grote mis
            if (dist > maxDistance * 0.9f)
                finalReward -= 0.5f;

            // Bonus bij zeer dichtbij
            if (dist < 0.3f)
                finalReward += 1f;

            AddReward(finalReward);
            EndEpisode();
        }

        // Lichte tijdstraf per stap
        AddReward(-0.0005f);
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
