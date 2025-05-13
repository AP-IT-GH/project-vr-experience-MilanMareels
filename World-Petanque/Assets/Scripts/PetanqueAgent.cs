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
        // Reset ball
        ballRb.transform.position = ballStartPos.position;
        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        // Randomize target position within a smaller range
        target.position = initialTargetPosition + new Vector3(
            Random.Range(-1.5f, 1.5f),
            0f,
            Random.Range(-1.5f, 1.5f)
        );

        hasThrown = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Relative position to target (3)
        Vector3 toTarget = target.position - ballRb.position;
        sensor.AddObservation(toTarget);

        // Ball velocity (3)
        sensor.AddObservation(ballRb.linearVelocity);

        // Ball and target positions (6)
        sensor.AddObservation(ballRb.position);
        sensor.AddObservation(target.position);

        // Total: 3 + 3 + 3 + 3 = 12 observations
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!hasThrown)
        {
            float x = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
            float z = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
            float power = Mathf.Clamp(actions.ContinuousActions[2], 0.1f, 1f) * maxThrowPower;
            float angle = Mathf.Clamp(actions.ContinuousActions[3], 0.1f, 0.6f);

            Vector3 direction = new Vector3(x, angle, z).normalized;
            ballRb.AddForce(direction * power, ForceMode.VelocityChange);

            hasThrown = true;
        }

        // Terminate if the ball falls below ground
        if (ballRb.position.y < groundLevel - 0.5f)
        {
            AddReward(-0.5f);
            EndEpisode();
            return;
        }

        // Give ongoing reward while ball is moving (closer = better)
        if (hasThrown && ballRb.linearVelocity.magnitude > 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float reward = Mathf.Clamp01(1f - (dist / maxDistance));
            AddReward(reward * 0.001f); // small shaping reward
        }

        // End episode when ball stops
        if (hasThrown && ballRb.linearVelocity.magnitude <= 0.05f)
        {
            float dist = Vector3.Distance(ballRb.position, target.position);
            float finalReward = Mathf.Clamp01(1f - (dist / maxDistance));

            if (dist < 0.3f)
                finalReward += 1f; // bonus for very close

            AddReward(finalReward);
            EndEpisode();
        }

        // Optional: lighter time penalty
        AddReward(-0.0005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal");
        actions[1] = Input.GetAxis("Vertical");
        actions[2] = Input.GetKey(KeyCode.Space) ? 1f : 0.7f;
        actions[3] = 0.4f;
    }
}
