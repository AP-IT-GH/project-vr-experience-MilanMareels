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

        // Reset target naar willekeurige plek binnen bereik
        target.position = initialTargetPosition + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f));

        hasThrown = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Relatieve positie target (3)
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
            float throwCommand = Mathf.Clamp01(actions.ContinuousActions[4]);

            if (throwCommand > 0.5f)
            {
                float dirX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
                float dirY = Mathf.Clamp(actions.ContinuousActions[1], 0.1f, 1f);
                float dirZ = Mathf.Clamp(actions.ContinuousActions[2], -1f, 1f);
                float power = Mathf.Clamp(actions.ContinuousActions[3], 0.1f, 1f) * maxThrowPower;

                Vector3 direction = new Vector3(dirX, dirY, dirZ).normalized;
                ballRb.AddForce(direction * power, ForceMode.VelocityChange);

                hasThrown = true;
            }

            // Straf als de agent blijft treuzelen
            AddReward(-0.001f);
            return;
        }

        // Straf als de bal uit de wereld valt
        if (ballRb.position.y < groundLevel - 0.5f)
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }

        // Shaping reward zolang bal beweegt
        if (ballRb.linearVelocity.magnitude > 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float reward = Mathf.Clamp01(1f - (dist / maxDistance));
            AddReward(reward * 0.01f);
        }

        // Episode eindigt zodra de bal stilstaat
        if (ballRb.linearVelocity.magnitude <= 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float normalizedDist = Mathf.Clamp01(dist / maxDistance);

            float finalReward = Mathf.Lerp(1f, -1f, normalizedDist); // Van +1 tot -1

            AddReward(finalReward);
            EndEpisode();
        }

        // Lichte tijdstraf per stap
        AddReward(-0.0005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");         // x-richting
        actions[1] = 0.5f;                                // y-richting vast (lichte boog)
        actions[2] = Input.GetAxis("Vertical");           // z-richting
        actions[3] = Input.GetKey(KeyCode.LeftShift) ? 1f : 0.6f; // kracht
        actions[4] = Input.GetKey(KeyCode.Space) ? 1f : 0f;       // gooi commando
    }
}
